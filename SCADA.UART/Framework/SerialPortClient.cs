using SCADA.Common;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace SCADA.UART.Framework
{
    public delegate RequestResult ReceivedEventHandler<TCharOrByte>(object sender, ResponseInfo<TCharOrByte> response) where TCharOrByte : struct, IConvertible, IComparable;

    /* 某次请求失败，表示通信接口出现致命问题，要标记缓存请求为取消，错误未重置前新的请求直接抛出异常，Monitor也要暂停工作。
     * 也要触发RequestFailed事件，供端口持有客户端标记自身错误，因为端口相当于客户端的核心零部件，核心故障，客户端也应当被标记成故障。
     */

    // 控制指令不要重试，查询指令允许重试
    public sealed class SerialPortClient<TCharOrByte> : ITimerDemander where TCharOrByte : struct, IConvertible, IComparable
    {
        private readonly IReceiveFilter<TCharOrByte> _filter;
        private readonly SerialPort _port;
        private readonly IValidator<TCharOrByte> _validator;
        private RequestInfo<TCharOrByte> _activeRequest;
        private readonly LinkedList<RequestInfo<TCharOrByte>> _normalPriorityLevelRequests;
        private int _id = int.MinValue;
        private readonly RequestResult _initialOperationResult = new RequestResult() { FailReason = FailReason.Processing, Data = null, FailDetail = null };
        private readonly LinkedList<RequestInfo<TCharOrByte>> _lowerPriorityLevelRequests;
        private readonly ThreadSafeReadHeavyList<TCharOrByte> _recvBuff = new ThreadSafeReadHeavyList<TCharOrByte>();
        private readonly ConcurrentDictionary<int, RequestResult> _results = new ConcurrentDictionary<int, RequestResult>();
        private readonly object _syncRoot;

        public SerialPortClient(IReceiveFilter<TCharOrByte> filter, IValidator<TCharOrByte> validator, CommunicationOptions options, string tag = "", string timerGroup = "")
        {
            if (typeof(TCharOrByte) != typeof(byte) && typeof(TCharOrByte) != typeof(char))
            {
                throw new ArgumentException("TCharOrByte must be byte or char");
            }
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (timerGroup == null) throw new ArgumentNullException(nameof(timerGroup));
            if (tag == null) throw new ArgumentNullException(nameof(tag));
            _filter = filter;
            _validator = validator;
            CommunicationOptions = options;

            _lowerPriorityLevelRequests = new LinkedList<RequestInfo<TCharOrByte>>();
            _syncRoot = ((ICollection)_lowerPriorityLevelRequests).SyncRoot;
            LastRecvResponseTime = DateTime.MinValue;
            _port = new SerialPort
            {
                PortName = options.PortName,
                BaudRate = options.BaudRate,
                Parity = options.Parity,
                StopBits = options.StopBits,
                DataBits = options.DataBits,
                RtsEnable = options.RtsEnable,
                Handshake = options.Handshake,
                ReadTimeout = 500,
                WriteTimeout = 500
            };
        }

        public event Action BreakdownOcurred;

        public event EventHandler<IList<TCharOrByte>> RawDataReceived;

        public event EventHandler<IList<TCharOrByte>> RawDataSent;

        /// <summary>
        /// 某次请求-响应失败时触发该事件
        /// </summary>
        public event EventHandler RequestFailed;

        public event ReceivedEventHandler<TCharOrByte> ResponseReceived;

        public bool Breakdown { get; private set; }
        public CommunicationOptions CommunicationOptions { get; }
        public DateTime LastRecvResponseTime { get; private set; }
        public int RequestCacheCapacity { get; set; } = 100;
        public string Tag { get; }
        public string TimerGroup { get; }
        private int ID => Interlocked.Increment(ref _id);

        public bool IsRequestProcessed(int id, out RequestResult operationResult)
        {
            operationResult = null;
            if (id == int.MaxValue)
            {
                operationResult = new RequestResult()
                {
                    FailReason = FailReason.CancelledForBreakdown,
                };
                return true;
            }
            if (_results.TryGetValue(id, out var result))
            {
                if (result.FailReason == FailReason.Processing)
                {
                    return false;
                }
                else
                {
                    operationResult = result;
                    return true;
                }
            }
            else
            {
                throw new ArgumentException("Invalid request ID.", nameof(id));
            }
        }

        public void Monitor()
        {
            if (Breakdown) return;

            // try to open port,it can only be executed once
            if (!_port.IsOpen)
            {
                _port.DataReceived += _port_DataReceived;
                try
                {
                    _port.Open();
                }
                catch
                {
                    _port.DataReceived -= _port_DataReceived;
                    CleanUp();
                }

                return;
            }

            // select active request

            if (_activeRequest == null) // 说明空闲
            {
                _activeRequest = _normalPriorityLevelRequests.FirstOrDefault();
                if (_activeRequest != null)
                {
                    _normalPriorityLevelRequests.RemoveFirst();
                }
                if (_activeRequest == null)
                {
                    _activeRequest = _lowerPriorityLevelRequests.FirstOrDefault();
                    if (_activeRequest != null)
                    {
                        _lowerPriorityLevelRequests.RemoveFirst();
                    }
                }
            }

            if (_activeRequest == null)
            {
                return; // 说明没有请求，直接返回
            }

            if (_activeRequest != null && _activeRequest.HasBeenSent == false)
            {
                ClearBuff();

                if (typeof(TCharOrByte) == typeof(byte))
                    _port.Write(_activeRequest.Contents.Cast<byte>().ToArray(), 0, _activeRequest.Contents.Count);
                else
                    _port.Write(_activeRequest.Contents.Cast<char>().ToArray(), 0, _activeRequest.Contents.Count);
                _activeRequest.SetSendStatus(true);
                _activeRequest.IncrementRetry();
                _activeRequest.RestartTimer();
                RawDataSent?.Invoke(this, _activeRequest.Contents);
                return;
            }

            // filter & validate & handler response
            var buff = _recvBuff.GetData();
            if (_filter.IsComplete(buff, _activeRequest.Contents))
            {
                LastRecvResponseTime = DateTime.Now;
                if (_validator.IsValid(buff))
                {
                    try
                    {
                        var operationResult = ResponseReceived?.Invoke(this, new ResponseInfo<TCharOrByte>(_activeRequest.CommandName, buff));
                        operationResult.FailReason = FailReason.HandlerSuccess;
                        _results.TryUpdate(_activeRequest.ID, operationResult, _initialOperationResult);
                        _activeRequest = null;
                    }
                    catch (Exception ex)
                    {
                        var operationResult = new RequestResult();
                        operationResult.FailReason = FailReason.HandlerException;
                        operationResult.Data = buff;
                        operationResult.FailDetail = ex.ToString();
                        _results.TryUpdate(_activeRequest.ID, operationResult, _initialOperationResult);
                        CleanUp();
                    }
                }
                else if (!_activeRequest.CanRetry)
                {
                    var operationResult = new RequestResult();
                    operationResult.FailReason = FailReason.ResponseValidatorFailed;
                    _results.TryUpdate(_activeRequest.ID, operationResult, _initialOperationResult);
                    _activeRequest = null;
                }
                else if (_activeRequest.CanRetry)
                {
                    _activeRequest.SetSendStatus(false);
                }
                return;
            }

            // retry or timeout
            if (_activeRequest != null)
            {
                if (_activeRequest.IsTimeout)
                {
                    if (_activeRequest.CanRetry)
                    {
                        _activeRequest.SetSendStatus(false);
                    }
                    else
                    {
                        _results.TryUpdate(_activeRequest.ID, new RequestResult() { FailReason = FailReason.ResponseTimeout, Data = buff },
                             _initialOperationResult);
                        CleanUp();
                    }
                }
            }
        }

        public int Request(IList<TCharOrByte> content, string commandName = "", PriorityLevel priority = PriorityLevel.Normal, int retryLimit = 0, int timeoutMS = 200)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (content.Count == 0)
            {
                throw new ArgumentException("Content cannot be empty.", nameof(content));
            }
            if (commandName == null)
            {
                throw new ArgumentNullException(nameof(commandName));
            }
            if (timeoutMS <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutMS), "Timeout must be greater than zero.");
            }
            if (retryLimit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryLimit), "Retry limit cannot be negative.");
            }
            if (Breakdown)
            {
                return int.MaxValue;
                //throw new InvalidOperationException("Serial port has a breakdown.");
            }
            int id;
            lock (_syncRoot)
            {
                if (_lowerPriorityLevelRequests.Count > RequestCacheCapacity)
                {
                    throw new InvalidOperationException("Serial port is too busy because request queue is full.");
                }
                id = ID;
                var reqinfo = new RequestInfo<TCharOrByte>(content, commandName, timeoutMS, retryLimit, id);
              
                _ = priority == PriorityLevel.Normal ? _normalPriorityLevelRequests.AddLast(reqinfo) : _lowerPriorityLevelRequests.AddLast(reqinfo);
            }
            // prune
            if (_results.Count > 2100)
            {
                foreach (var key in _results.Keys.OrderBy(key => key).ToArray().Take(100))
                {
                    _results.TryRemove(key, out _);
                }
            }
            _results.TryAdd(id, new RequestResult() { FailReason = FailReason.Processing });
            return id;
        }

        public void Reset()
        {
            Breakdown = false;
        }

        public void Send(IList<TCharOrByte> content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (content.Count == 0)
            {
                throw new ArgumentException("Content cannot be empty.", nameof(content));
            }
            if (typeof(TCharOrByte) == typeof(byte))
                _port.Write(content.Cast<byte>().ToArray(), 0, content.Count);
            else
                _port.Write(content.Cast<char>().ToArray(), 0, content.Count);
            RawDataSent?.Invoke(this, content);
        }

        private void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = (SerialPort)sender;
            int bytesToRead = 0;
            while ((bytesToRead = sp.BytesToRead) > 0)
            {
                if (bytesToRead > 0)
                {
                    if (typeof(TCharOrByte) == typeof(byte))
                    {
                        byte[] buffer = new byte[bytesToRead];
                        int count = sp.Read(buffer, 0, bytesToRead);
                        var actualReadData = buffer.Cast<TCharOrByte>().ToList().GetRange(0, count);
                        _recvBuff.AddRange(actualReadData);
                        RawDataReceived?.Invoke(this, actualReadData);
                    }
                    else if (typeof(TCharOrByte) == typeof(char))
                    {
                        char[] buffer = new char[bytesToRead];
                        int count = sp.Read(buffer, 0, bytesToRead);
                        var actualReadData = buffer.Cast<TCharOrByte>().ToList().GetRange(0, count);
                        _recvBuff.AddRange(actualReadData);
                        RawDataReceived?.Invoke(this, actualReadData);
                    }
                }
            }
        }

        private void CleanUp()
        {
            Breakdown = true;
            _activeRequest = null;
            BreakdownOcurred?.Invoke();
            foreach (var req in _lowerPriorityLevelRequests)
            {
                _results.TryUpdate(req.ID, new RequestResult() { FailReason = FailReason.CancelledForBreakdown }, _initialOperationResult);
            }
            _lowerPriorityLevelRequests.Clear();
            _recvBuff.Clear();
        }

        private void ClearBuff()
        {
            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();
            _recvBuff.Clear();
        }

        // 接收数据还用DataRecvieved事件。
        // 定时器负责：1. 打开串口 2. 发送请求 3. 检查是否超时 4. 重新连接 5. 完整包的拆包 6. 校验 7. 触发Handler 日志记录事件
        // 把所有的回调都放在一个列表中，每个定时器固定执行某几个回调。建议10个一组，且具有报表功能，能根据报表调整回调的顺序，频率，周期等。// 用不可变集合来存储回调列表，避免多线程问题，性能高于其他方案
        // 主定时器除了负责回调，还负责管理其他定时器
        // 高级功能，每个串口都有一个标识，标识相同的串口可以共享一个定时器
    }
}