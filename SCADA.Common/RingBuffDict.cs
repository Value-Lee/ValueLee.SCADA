using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/* 关于 ID 溢出：long 类型的最大值是 $2^{63}-1$，即便您每秒产生 100 万个 ID，也需要 29 万年 才会溢出。所以您可以放心使用递增 ID。
 * 容量监控：您可以尝试在 SetResponse 返回 false 时记录日志，这通常意味着您的 Capacity 设置小了，或者 PLC/下位机响应太慢，导致数据在被查询前就被环形覆盖了。
 * 通过 State 变量（Empty -> Registered -> Responded），我们实现了一个轻量级的原子锁。这确保了：
 * 覆盖安全性：当 Register 正在覆盖一个位置时，TryTake 不可能读到一半的无效数据。
 * 消费唯一性：即便多个 UI 线程同时尝试 TryTake 同一个 ID，CompareExchange 也能保证结果只会被消费一次。
 * 零分配：一旦 new RingMessageTracker 完成，后续所有的 Register 和 TryTake 都不再触发任何堆内存分配（Heap Allocation），极大减轻了工业软件中常见的 GC 抖动问题。
 * RingBufferTracker比ConcurrentDictionary等通用数据结构性能更优(快10倍以上)，适合高频率的注册与查询场景。
 * 字典需要计算 id.GetHashCode() 并处理可能的哈希冲突。RingBuffer 只需要执行一次位运算 id & mask。
 * RingBuffer 是一个固定的数组，没有任何新对象分配，GC 压力为 0。
 * 字典每插入一个新元素，都要创建一个 Node 对象（如果是引用类型），这会产生大量零散内存。
 * ConcurrentDictionary 在清理时需要遍历 Keys。随着字典增大，遍历变得越来越慢。
 * 更糟糕的是，遍历 Keys 会触发字典内部的快照机制或锁定，严重阻塞生产线程。
 *
 *
 * 在 C# 中，struct 是值类型。当你创建一个 Slot[] 数组时，CLR 会在堆上分配一块连续的内存。

连续性：所有的 Slot 都在一块物理内存里。

无分配（Zero Allocation）：当你执行 _buffer[index] = new Slot(...) 时，C# 编译器实际上并没有在堆上申请新空间，它只是原地修改（Overwrite）了那块内存里的字节。

GC 友好：整个 RingMessageTracker 在其生命周期内只产生 1次 数组分配。无论你注册 100 万次还是 1 亿次，都不会产生新的 GC 碎片。

1.2 对象池的本质
对象池（Object Pool）是为了解决 class（引用类型）频繁 new 导致的 GC 压力。

如果你把 Slot 改成 class，那么数组里存储的就只是指针，每个指针指向堆上独立的对象。这时才需要对象池。

当前的 struct 数组设计其实就是一个“物理级”的对象池。

虽然 Slot 不需要优化，但 TResult 可能会成为瓶颈。 如果你的 TResult 是一个复杂的类（比如 class MessageResponse），且你每秒生成 200 个响应：

痛点：每次收到 PLC 答复，你可能都会 new MyResponse(...)。这才是产生 GC 压力的根源。

优化方案：对 TResult 使用 Microsoft.Extensions.ObjectPool 或自定义简单的池。

如果你的响应数据很小（比如只是几个 int 或 double），可以直接把 TResult 定义为 struct，并修改 RingMessageTracker 取消 where TResult : class 的限制。这样整个链路完全没有引用类型，实现真正的物理零拷贝。

// 响应到达时，从池里借一个对象
var response = _resultPool.Get();
response.Data = rawBytes;

// 存入 Tracker
tracker.SetResponse(id, response);

// 消费完后，手动归还
var result = tracker.TryTake(id);
if (result != null) {
    Process(result);
    _resultPool.Return(result); // 归还对象
}
 */

namespace SCADA.Common
{
    /*   !!!!!!!!!!!!!!!!  此类能正常工作的前提是:ID必须永远递增且不重复 !!!!!!!!!!!!!!   */

    /// <summary>
    /// 基于环形缓冲区的高性能递增 ID 消息追踪器。
    /// 适用于多线程高频注册、响应与查询场景，自动淘汰旧数据。
    /// </summary>
    /// <typeparam name="TResult">响应结果的类型（引用类型）</typeparam>
    public class RingBuffDict<TResult>
    {
        // 状态常量
        private const int StateEmpty = 0;
        private const int StateRegistered = 1;
        private const int StateResponded = 2;
        private readonly Slot[] _buffer;
        private readonly int _mask;
        private long _id = long.MinValue;

        /// <summary>
        /// 初始化追踪器
        /// </summary>
        /// <param name="capacityPow2">容量，必须是 2 的幂（如 1024, 4096, 65536）</param>
        public RingBuffDict(int capacityPow2)
        {
            if (capacityPow2 < 1 || (capacityPow2 & (capacityPow2 - 1)) != 0)
                throw new ArgumentException("Capacity must be a power of 2 (e.g., 1024, 2048, 4096).");

            _buffer = new Slot[capacityPow2];
            _mask = capacityPow2 - 1;
        }

        /// <summary>
        /// 注册一个新生成的 ID。如果该位置已有旧数据，将被直接覆盖。
        /// </summary>
        public long Register()
        {
            long id = Interlocked.Increment(ref _id);
            int index = (int)(id & _mask);

            // 1. 原子性地重置状态，阻止其他线程此时读取或写入结果
            Interlocked.Exchange(ref _buffer[index].State, StateEmpty);

            // 2. 覆盖数据（此步不需要原子操作，因为状态已被重置为 Empty）
            _buffer[index].Id = id;
            _buffer[index].Result = default;

            // 3. 发布状态为“已注册”
            Interlocked.Exchange(ref _buffer[index].State, StateRegistered);

            return id;
        }

        /// <summary>
        /// 响应者存入结果。
        /// </summary>
        /// <returns>若 ID 匹配且存入成功返回 true；若 ID 已过期被覆盖则返回 false</returns>
        public bool SetResponse(long id, TResult result)
        {
            int index = (int)(id & _mask);

            // 只有当 ID 匹配且状态仍为 Registered 时才写入
            if (Volatile.Read(ref _buffer[index].Id) == id && _buffer[index].State == StateRegistered)
            {
                _buffer[index].Result = result;
                // 尝试切换到 Responded 状态
                return Interlocked.CompareExchange(ref _buffer[index].State, StateResponded, StateRegistered) == StateRegistered;
            }
            return false;
        }

        /// <summary>
        /// 尝试获取响应结果。一旦获取成功，该位置将被标记为过期。
        /// </summary>
        public bool TryTake(long id, out TResult result)
        {
            int index = (int)(id & _mask);
            result = default;
            // 快速检查 ID 和状态
            if (Volatile.Read(ref _buffer[index].Id) == id && _buffer[index].State == StateResponded)
            {
                // 尝试将状态从 Responded 改为 Empty，确保只有一个线程能成功 Take
                if (Interlocked.CompareExchange(ref _buffer[index].State, StateEmpty, StateResponded) == StateResponded)
                {
                    TResult res = _buffer[index].Result;
                    _buffer[index].Result = default; // 显式置空，帮助 GC 回收
                    result = res;
                    return true;
                }
            }
            return false;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private struct Slot
        {
            public long Id;          // 存储的唯一消息ID
            public TResult Result;   // 响应结果引用
            public int State;        // 内部状态流转
        }
    }
}