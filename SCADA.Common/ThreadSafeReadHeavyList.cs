using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA.Common
{
    public class ThreadSafeReadHeavyList<T>
    {
        // 使用 volatile 或 Interlocked 来确保引用的可见性和原子性更新
        private ImmutableList<T> _data = ImmutableList<T>.Empty;

        public int Count => _data.Count;

        // 写入操作：创建一个新集合并原子性地替换引用
        public void Add(T item)
        {
            // 这是一个自旋锁，确保更新是基于最新的数据快照
            while (true)
            {
                // 1. 获取当前引用
                var originalData = _data;
                // 2. 基于当前数据创建新集合
                var newData = originalData.Add(item);
                // 3. 原子地比较并交换。如果_data没有被其他写线程改变，就更新它
                if (Interlocked.CompareExchange(ref _data, newData, originalData) == originalData)
                {
                    // 更新成功，退出循环
                    break;
                }
                // 如果失败（意味着另一个写线程抢先更新了），循环会重试
            }
        }

        // 写入操作：创建一个新集合并原子性地替换引用
        public void AddRange(IEnumerable<T> items)
        {
            // 这是一个自旋锁，确保更新是基于最新的数据快照
            while (true)
            {
                // 1. 获取当前引用
                var originalData = _data;
                // 2. 基于当前数据创建新集合
                var newData = originalData.AddRange(items);
                // 3. 原子地比较并交换。如果_data没有被其他写线程改变，就更新它
                if (Interlocked.CompareExchange(ref _data, newData, originalData) == originalData)
                {
                    // 更新成功，退出循环
                    break;
                }
                // 如果失败（意味着另一个写线程抢先更新了），循环会重试
            }
        }

        public void Clear()
        {
            while (true)
            {
                var originalData = _data;
                var newData = ImmutableList<T>.Empty;
                if (Interlocked.CompareExchange(ref _data, newData, originalData) == originalData)
                {
                    break;
                }
            }
        }

        // 通过索引安全地获取元素
        public T Get(int index)
        {
            // 直接访问，因为 _data 引用在读取期间是稳定的
            return _data[index];
        }

        // 读取操作：极其快速，无锁
        public ImmutableList<T> GetData()
        {
            return _data;
        }

        public void Remove(T item)
        {
            while (true)
            {
                var originalData = _data;
                var newData = originalData.Remove(item);
                if (Interlocked.CompareExchange(ref _data, newData, originalData) == originalData)
                {
                    break;
                }
            }
        }
    }
}