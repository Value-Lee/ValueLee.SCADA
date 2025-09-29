//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ConsoleApp1
//{

//    #region 协议包
//    public interface IPackageInfo
//    {

//    }

//    public interface IPackageInfo<T> : IPackageInfo
//    {
//        /// <summary>
//        ///  Key对于Handler具有很重要的意义，Handler必须要根据Key来处理不同的协议
//        /// </summary>
//        T Key { get; }
//    }
//    #endregion

//    #region Filter
//    /// <summary>
//    /// Filter的职责是将字节流拆包成一个个的PackageInfo，所以底层通信协议必须要有特征以便拆包
//    /// </summary>
//    /// <typeparam name="TPackageInfo"></typeparam>
//    public interface IReceiveFilter<out TPackageInfo> where TPackageInfo : IPackageInfo
//    {

//    }

//    public interface IReceiveFilter<IValidate, out TPackageInfo> where TPackageInfo : IPackageInfo
//    {

//    }
//    #endregion

//    #region Validate
//    /// <summary>
//    /// 对于不可靠的物理层，比如无线通信，可能会收到错误的字节流，这时候需要对字节流进行校验，但是如果是TCP这种可靠的物理层，就不需要校验
//    /// </summary>
//    public interface IValidator
//    {
//        bool Validate(byte[] data);
//    }
//    #endregion

//    #region Handler

//    public interface IHandler<TPackageInfo> where TPackageInfo : IPackageInfo
//    {
//        void Handle(TPackageInfo package);
//    }

//    #endregion

//    public class Client<TReceiveFilter> where TReceiveFilter : IReceiveFilter<IPackageInfo>
//    {
//        public Client(Action<IPackageInfo> handler)
//        {
            
//        }

//        public void Send(byte[] data)
//        {
//        }
//        /// <summary>
//        /// 字典法消除大量的IF ELSE
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name=""></param>
//        public void Add(string key, IHandler<TPackageInfo>)
//        {

//        }
//    }

//    /* 总结
//     * 良好的通信协议应当满足以下几点
//     * 1. 协议必须要有 req_id 可以实现并发，响应方必须要带上请求的 req_id，这样就能定位到是哪个请求的响应。
//     *    没有 req_id 只能串行，因为当前的响应必然是最后一次请求的响应。但是可以通过多连接实现并发，还可以实现伪并发，
//     *    使用一个缓存区，多次的发送请求时将请求放入缓存区，请求无需等待响应才发送下一个请求，
//     *    响应时从缓存区取出请求进行处理，删除对头，再发送缓存的下一个请求，这样就能实现伪并发，上层看是并发的，实际上底层是串行的。
//     *    
//     * 2. 典型的响应的上层对象包，结构一般是Key + Parameter1 + Parameter2 + Parameter3 + ...
//     *    Key表示功能，也就是方法名，Parameter是方法参数。收到的响应必须要知道它相应的功能，也就是命令Key，
//     *    这样才能知道如何处理这个响应。命令字可以是响应方包含在协议中，从协议中的某些字节拿到Key，
//     *    有些协议中不包含代表Key的特殊字节，那么也可以是请求方知道发送的请求的功能，从而知道响应的功能，但这需要知道响应对应的请求内容！
//     *    
//     * 3. 协议必须要有特征以便拆包，Modbus TCP 有长度域，这样就能知道一个完整的包的长度，从而拆包。如果协议没有
//     *    特征，就需要知道响应对应的请求字节，这样就能知道响应的长度，从而拆包，比如 Modbus RTU ，虽然可以用时间间隔来拆包，但是效率太低。
//     *    
//     * 4. Handler有大量的IF ELSE，违背了开闭原则，可以考虑使用反射或者字典来实现Handler
//     */
//}
