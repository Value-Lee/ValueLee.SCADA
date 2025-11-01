using MoreLinq;
using Newtonsoft.Json.Linq;
using SCADA.Common;
using SCADA.ObjectModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SCADA.PLCFramework
{
    public class PlcAdapterWrapper : IDevice, IConnection, IPlcAccessor
    {
        private Channel<string> WriteCommandQueue = Channel.CreateUnbounded<string>();
        private Channel<string> ReadCommandQueue = Channel.CreateUnbounded<string>();

        private readonly ConcurrentQueue<CommandInfo> _commandQueue;
        private readonly ConcurrentDictionary<string, (OperationResult<object> result, long timeStamp)> _registItemValueCache;
        private readonly ConcurrentDictionary<int, TaskCompletionSource<object>> _registItemValueWaiter;

        private readonly Dictionary<string, RegistItem> _registItems;

        private readonly Dictionary<Block, IList<string>> _addrsOfBlock;
        private readonly double _ticksToMillisecondsFactor = 1000.0 / Stopwatch.Frequency;

        private int _id = int.MinValue;
        private int GetId()
        {
            return Interlocked.Increment(ref _id);
        }

        private readonly IPlcAdapter _plcAdapter;
        private readonly PlcInfo _plcInfo;

        public PlcAdapterWrapper(PlcInfo plcInfo)
        {
            _commandQueue = new ConcurrentQueue<CommandInfo>();
            _registItemValueCache = new ConcurrentDictionary<string, (OperationResult<object> result, long timeStamp)>();
            _registItemValueWaiter = new ConcurrentDictionary<int, TaskCompletionSource<object>>();
            _addrsOfBlock = new Dictionary<Block, IList<string>>();
            _registItems = new Dictionary<string, RegistItem>();
            _plcInfo = plcInfo;
            if (plcInfo.Blocks != null)
                plcInfo.Blocks.ForEach(block => _addrsOfBlock.Add(block, _plcAdapter.ResolveAllAddrs(block.StartAddr, block.Type, block.Len)));
            if (plcInfo.DIs != null)
                plcInfo.DIs.ForEach(di => _registItems.Add(di.Key, di.Value));
            if (plcInfo.DOs != null)
                plcInfo.DOs.ForEach(@do => _registItems.Add(@do.Key, @do.Value));
            if (plcInfo.AIs != null)
                plcInfo.AIs.ForEach(ai => _registItems.Add(ai.Key, ai.Value));
            if (plcInfo.AOs != null)
                plcInfo.AOs.ForEach(ao => _registItems.Add(ao.Key, ao.Value));
            _plcAdapter = Assembly.LoadFrom(_plcInfo.Assembly).CreateInstance(_plcInfo.Class) as IPlcAdapter;
        }

        private void CheckNameExist(string name)
        {
            if (!_registItems.ContainsKey(name))
            {
                throw new ArgumentException($"{name} doesn't exist in plc({_plcInfo.IP}:{_plcInfo.Port}).");
            }
        }

        private object GetValueFromCache(string name, int cacheExpiryTime,long nowTicks)
        {
            if (_registItemValueCache.TryGetValue(name, out var value))
            {
                if ((nowTicks - value.timeStamp) * _ticksToMillisecondsFactor <= cacheExpiryTime)
                {
                    return value;
                }
            }
            return null;
        }

        private bool GetBlockInclude(string addr, out Block block)
        {
            foreach (var b in _addrsOfBlock.Keys)
            {
                var addrs = _addrsOfBlock[b];
                if (addrs.Contains(addr))
                {
                    block = b;
                    return false;
                }
            }
            block = null;
            return true;
        }

        string IConnection.Address { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Initialize()
        {
            _plcAdapter.Connect();

            if (_plcInfo.Blocks != null)
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        // todo 添加计时器，如果一次循环少于10ms，
                        var list = new List<CommandInfo>();
                        int count = 0;
                        // 一次性把队列中的命令取干净，取的越多，整体的读写速度越高。但是做个限制，最多读1000个，这个上限值是否需要以及是否最合适，待验证。
                        // 假设100个命令，Block配置8个，8读2写=10次网络交互，开销是10×10ms=100ms，除以100，平均每个读或写操作是1ms。
                        // 假设最差情况是10个命令，平均是10ms. 10ms能够完成一次读或写，已经足够快了。即便是阻塞性先后读5次，也只是50ms，完全能够算的上是瞬时操作。
                        while (count > 999)
                        {
                            if (_commandQueue.TryDequeue(out var command))
                            {
                                list.Add(command);
                                count++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (list.Count > 0)
                        {
                            #region 先处理读

                            #region ① 处理允许从缓存读取的
                            for (int i = 0; i < list.Count; i++)
                            {
                                // 批量读，使用同一个时间戳，可以避免应该从同一个快照的数字IO被分成两个快照
                                long nowTicks = Stopwatch.GetTimestamp();

                                if (list[i].Operation == Operation.Read && list[i].AllowFromCache == true)
                                {
                                    for (int j = 0; j < list[i].Names.Count; j++)
                                    {
                                        object value = GetValueFromCache(list[i].Names[j], list[i].CacheExpiryTime, nowTicks);
                                        if (value != null)
                                        {
                                            list[i].Values[j] = value;
                                        }
                                    }
                                    if (list[i].Values.All(x => x != null))
                                    {
                                        if (list[i].Values.Count > 1)
                                        {
                                            _registItemValueWaiter[list[i].ID].SetResult(list[i].Values);
                                        }
                                        else
                                        {
                                            _registItemValueWaiter[list[i].ID].SetResult(list[i].Values[0]);
                                        }
                                        _registItemValueWaiter.TryRemove(list[i].ID, out _);
                                    }
                                }
                            }

                            //  删除已经处理完毕的
                            for (int i = list.Count - 1; i >= 0; i--)
                            {
                                if (list[i].Values.All(x => x != null))
                                {
                                    list.RemoveAt(i);
                                }
                            }
                            #endregion

                            #region ② 处理不允许从缓存读的和缓存失效的
                            List<Block> blocks = new List<Block>();
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (list[i].Operation == Operation.Read)
                                {
                                    for (int j = 0; j < list.Count; j++)
                                    {
                                        if (list[i].Values[j] == null)
                                        {
                                            if (!GetBlockInclude(list[i].Names[j], out Block block))
                                            {
                                                blocks.Add(block);
                                            }
                                        }
                                    }
                                }
                            }
                            // 读取Block
                            #endregion

                            #region ③ 零散读取不能整合到Block批量读取的地址

                            #endregion
                            #endregion

                            #region 后处理写
                            // 命中Block,就借助Block批量写， Block的其他值从缓存中获取，获取不到的话，产生一次读取
                            // 未命中Block的地址，单独产生一次写
                            #endregion
                        }
                    }
                });
            }
        }

        public void Reset()
        {
            _plcAdapter.Disconnect();
            _plcAdapter.Connect();
        }

        public void Terminate()
        {
        }

        OperationResult IConnection.Connect()
        {
            throw new NotImplementedException();
        }

        Task<OperationResult> IConnection.ConnectAsync()
        {
            throw new NotImplementedException();
        }

        OperationResult IConnection.Disconnect()
        {
            throw new NotImplementedException();
        }

        Task<OperationResult> IConnection.DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        #region IPlcAccessor
        public int Read(params string[] names)
        {
            return Read(true, 100, names);
        }

        public int Read(bool allowFromCache, params string[] names)
        {
            return Read(allowFromCache, 100, names);
        }

        public int Read(bool allowFromCache, int cacheExpiryTime, params string[] names)
        {
            names.ForEach(x => CheckNameExist(x));
            int id = GetId();
            _commandQueue.Enqueue(new CommandInfo()
            {
                ID = id,
                Names = names,
                Values = new object[names.Length],
                AllowFromCache = allowFromCache,
                CacheExpiryTime = cacheExpiryTime,
                Operation = Operation.Read
            });
            _registItemValueWaiter.TryAdd(id, new TaskCompletionSource<object>());
            return id;
        }

        public int Write(string name, object value)
        {
            CheckNameExist(name);
            int id = GetId();
            _commandQueue.Enqueue(new CommandInfo()
            {
                ID = id,
                Names = new string[] { name },
                Values = new object[] { value },
                Operation = Operation.Write
            });
            _registItemValueWaiter.TryAdd(id, new TaskCompletionSource<object>());
            return id;
        }

        public int Write(string name1, object value1, string name2, object value2)
        {
            CheckNameExist(name1);
            CheckNameExist(name2);

            int id = GetId();
            _commandQueue.Enqueue(new CommandInfo()
            {
                ID = id,
                Names = new string[] { name1, name2 },
                Values = new object[] { value1, value2 },
                Operation = Operation.Write
            });
            _registItemValueWaiter.TryAdd(id, new TaskCompletionSource<object>());
            return id;
        }

        public int Write(string name1, object value1, string name2, object value2, string name3, object value3)
        {
            CheckNameExist(name1);
            CheckNameExist(name2);
            CheckNameExist(name3);
            int id = GetId();
            _commandQueue.Enqueue(new CommandInfo()
            {
                ID = id,
                Names = new string[] { name1, name2, name3 },
                Values = new object[] { value1, value2, value3 },
                Operation = Operation.Write
            });
            _registItemValueWaiter.TryAdd(id, new TaskCompletionSource<object>());
            return id;
        }

        public int Write(string name1, object value1, string name2, object value2, string name3, object value3, string name4, object value4)
        {
            CheckNameExist(name1);
            CheckNameExist(name2);
            CheckNameExist(name3);
            CheckNameExist(name4);
            int id = GetId();
            _commandQueue.Enqueue(new CommandInfo()
            {
                ID = id,
                Names = new string[] { name1, name2, name3, name4 },
                Values = new object[] { value1, value2, value3, value4 },
                Operation = Operation.Write
            });
            _registItemValueWaiter.TryAdd(id, new TaskCompletionSource<object>());
            return id;
        }

        public int Write(string name1, object value1, string name2, object value2, string name3, object value3, string name4, object value4, string name5, object value5)
        {
            CheckNameExist(name1);
            CheckNameExist(name2);
            CheckNameExist(name3);
            CheckNameExist(name4);
            CheckNameExist(name5);
            int id = GetId();
            _commandQueue.Enqueue(new CommandInfo()
            {
                ID = id,
                Names = new string[] { name1, name2, name3, name4, name5 },
                Values = new object[] { value1, value2, value3, value4, value5 },
                Operation = Operation.Write
            });
            _registItemValueWaiter.TryAdd(id, new TaskCompletionSource<object>());
            return id;
        }

        public int Write(params (string name, object value)[] nameValues)
        {
            nameValues.ForEach(x => CheckNameExist(x.name));
            int id = GetId();
            _commandQueue.Enqueue(new CommandInfo()
            {
                ID = id,
                Names = nameValues.Select(x => x.name).ToArray(),
                Values = nameValues.Select(x => x.value).ToArray(),
                Operation = Operation.Write
            });
            _registItemValueWaiter.TryAdd(id, new TaskCompletionSource<object>());
            return id;
        }

        public bool CheckResult(int id, out OperationResult value)
        {
            if (_registItemValueWaiter.TryGetValue(id, out var tcs))
            {
                if (tcs.Task.IsCompleted)
                {
                    value = (tcs.Task.GetAwaiter().GetResult()) as OperationResult;
                    _registItemValueWaiter.TryRemove(id, out var _);
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
            else
            {
                throw new Exception($"ID:{id} not found.");
            }
        }

        public async Task<OperationResult> WaitResult(int id)
        {
            if (_registItemValueWaiter.TryGetValue(id, out var tcs))
            {
                var ret = (await tcs.Task.ConfigureAwait(false)) as OperationResult;
                _registItemValueWaiter.TryRemove(id, out var _);
                return ret;
            }
            else
            {
                throw new Exception($"ID:{id} not found.");
            }
        }
        #endregion
    }
}
