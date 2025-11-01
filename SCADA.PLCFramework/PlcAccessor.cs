using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq;
using Newtonsoft.Json;
using SCADA.Common;
using SCADA.ObjectModel;

namespace SCADA.PLCFramework
{
    public class PlcAccessor : IPlcAccessor, IDevice
    {
        private readonly ConcurrentDictionary<string, object> _registValueCache;
        private List<PlcInfo> _plcInfos;



        private PlcAccessor()
        {
            _registValueCache = new ConcurrentDictionary<string, object>();
            _plcInfos = new List<PlcInfo>();
        }

        private static PlcAccessor Instance { get; } = new PlcAccessor();

        #region 加载PLC
        public void LoadNewPlc(PlcInfo plcInfo)
        {
            // 数字输入和输出的名称不能重复

            if (plcInfo == null)
            {
                throw new ArgumentNullException(nameof(plcInfo));
            }

            HashSet<string> names = new HashSet<string>();
            _plcInfos.ForEach(plc => plc.DIs.ForEach(di => names.Add(di.Key)));
            plcInfo.DIs.ForEach(di =>
            {
                if (!names.Contains(di.Key))
                {
                    names.Add(di.Key);
                }
                else
                {
                    throw new ArgumentException($"di {di.Key} aleady exists.");
                }
            });

            names.Clear();

            _plcInfos.ForEach(plc => plc.DOs.ForEach(@do => names.Add(@do.Key)));
            plcInfo.DOs.ForEach(@do =>
            {
                if (!names.Contains(@do.Key))
                {
                    names.Add(@do.Key);
                }
                else
                {
                    throw new ArgumentException($"do {@do.Key} aleady exists.");
                }
            });
            names.Clear();

            _plcInfos.ForEach(plc => plc.AIs.ForEach(ai => names.Add(ai.Key)));
            plcInfo.AIs.ForEach(ai =>
            {
                if (!names.Contains(ai.Key))
                {
                    names.Add(ai.Key);
                }
                else
                {
                    throw new ArgumentException($"ai {ai.Key} aleady exists.");
                }
            });
            names.Clear();

            _plcInfos.ForEach(plc => plc.AOs.ForEach(ao => names.Add(ao.Key)));
            plcInfo.AOs.ForEach(ao =>
            {
                if (!names.Contains(ao.Key))
                {
                    names.Add(ao.Key);
                }
                else
                {
                    throw new ArgumentException($"ao {ao.Key} aleady exists.");
                }
            });
            names.Clear();

            _plcInfos.Add(plcInfo);
        }
        public void LoadNewPlc(string plcInfoFile)
        {
            var plcInfoCollection = JsonConvert.DeserializeObject<List<PlcInfo>>(plcInfoFile);
            foreach (var plcInfo in plcInfoCollection)
            {
                LoadNewPlc(plcInfo);
            }
        }

        public PlcInfo[] GetPlcInfos() => JsonSerializer.Deserialize<PlcInfo[]>(JsonSerializer.Serialize(_plcInfos));
        #endregion

        public void Initialize()
        {
            foreach (var plcInfo in _plcInfos)
            {
                plcInfo.PlcAdapter = Assembly.LoadFrom(plcInfo.Assembly).CreateInstance(plcInfo.Class) as IPlcAdapter;
            }
            foreach (var plcInfo in _plcInfos)
            {
                plcInfo.PlcAdapter.Connect();
            }
            foreach (var plcInfo in _plcInfos)
            {
                if (plcInfo.Blocks != null)
                {
                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            foreach (var block in plcInfo.Blocks)
                            {
                                var keys = new HashSet<string>(_registValueWaiter.Keys);

                                var addrs = plcInfo.PlcAdapter.ResolveAddrFormat(block.StartAddr, block.Type, block.Len);
                                try
                                {
                                    switch (block.Type)
                                    {
                                        case ValueType.@bool:

                                            var bools = await plcInfo.PlcAdapter.ReadBoolsAsync(block.StartAddr, block.Len);

                                            if (bools.IsSuccess)
                                            {
                                                for (int i = 0; i < addrs.Length; i++)
                                                {
                                                    // 更新缓存
                                                    _registValueCache.AddOrUpdate(addrs[i], (key) => bools.Data[i], (key, oldvalue) => bools.Data[i]);
                                                    // 释放block
                                                    if (keys.Contains(addrs[i]))
                                                    {
                                                        var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                                        if (exist)
                                                        {
                                                            tcs.SetResult(bools.Data[i]);
                                                            _registValueWaiter.TryRemove(addrs[i], out tcs);
                                                        }
                                                    }
                                                }
                                            }
                                            break;

                                        case ValueType.int8:

                                            var int8s = await plcInfo.PlcAdapter.ReadInt8sAsync(block.StartAddr, block.Len);

                                            if (int8s.IsSuccess)
                                            {
                                                for (int i = 0; i < addrs.Length; i++)
                                                {
                                                    // 更新缓存
                                                    _registValueCache.AddOrUpdate(addrs[i], (key) => int8s.Data[i], (key, oldvalue) => int8s.Data[i]);
                                                    // 释放block
                                                    if (keys.Contains(addrs[i]))
                                                    {
                                                        var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                                        if (exist)
                                                        {
                                                            tcs.SetResult(int8s.Data[i]);
                                                            _registValueWaiter.TryRemove(addrs[i], out tcs);
                                                        }
                                                    }
                                                }
                                            }
                                            break;

                                        case ValueType.uint8:

                                            var uint8s = await plcInfo.PlcAdapter.ReadUint8sAsync(block.StartAddr, block.Len);

                                            if (uint8s.IsSuccess)
                                            {
                                                for (int i = 0; i < addrs.Length; i++)
                                                {
                                                    // 更新缓存
                                                    _registValueCache.AddOrUpdate(addrs[i], (key) => uint8s.Data[i], (key, oldvalue) => uint8s.Data[i]);
                                                    // 释放block
                                                    if (keys.Contains(addrs[i]))
                                                    {
                                                        var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                                        if (exist)
                                                        {
                                                            tcs.SetResult(uint8s.Data[i]);
                                                            _registValueWaiter.TryRemove(addrs[i], out tcs);
                                                        }
                                                    }
                                                }
                                            }
                                            break;

                                        case ValueType.int16:

                                            var int16s = await plcInfo.PlcAdapter.ReadInt16sAsync(block.StartAddr, block.Len);

                                            if (int16s.IsSuccess)
                                            {
                                                for (int i = 0; i < addrs.Length; i++)
                                                {
                                                    // 更新缓存
                                                    _registValueCache.AddOrUpdate(addrs[i], (key) => int16s.Data[i], (key, oldvalue) => int16s.Data[i]);
                                                    // 释放block
                                                    if (keys.Contains(addrs[i]))
                                                    {
                                                        var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                                        if (exist)
                                                        {
                                                            tcs.SetResult(int16s.Data[i]);
                                                            _registValueWaiter.TryRemove(addrs[i], out tcs);
                                                        }
                                                    }
                                                }
                                            }
                                            break;

                                        case ValueType.uint16:

                                            var uint16s = await plcInfo.PlcAdapter.ReadUint16sAsync(block.StartAddr, block.Len);

                                            if (uint16s.IsSuccess)
                                            {
                                                for (int i = 0; i < addrs.Length; i++)
                                                {
                                                    // 更新缓存
                                                    _registValueCache.AddOrUpdate(addrs[i], (key) => uint16s.Data[i], (key, oldvalue) => uint16s.Data[i]);
                                                    // 释放block
                                                    if (keys.Contains(addrs[i]))
                                                    {
                                                        var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                                        if (exist)
                                                        {
                                                            tcs.SetResult(uint16s.Data[i]);
                                                            _registValueWaiter.TryRemove(addrs[i], out tcs);
                                                        }
                                                    }
                                                }
                                            }

                                            break;

                                        case ValueType.int32:

                                            var int32s = await plcInfo.PlcAdapter.ReadInt32sAsync(block.StartAddr, block.Len);

                                            if (int32s.IsSuccess)
                                            {
                                                for (int i = 0; i < addrs.Length; i++)
                                                {
                                                    // 更新缓存
                                                    _registValueCache.AddOrUpdate(addrs[i], (key) => int32s.Data[i], (key, oldvalue) => int32s.Data[i]);
                                                    // 释放block
                                                    if (keys.Contains(addrs[i]))
                                                    {
                                                        var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                                        if (exist)
                                                        {
                                                            tcs.SetResult(int32s.Data[i]);
                                                            _registValueWaiter.TryRemove(addrs[i], out tcs);
                                                        }
                                                    }
                                                }
                                            }

                                            break;

                                        case ValueType.uint32:

                                            var uint32s = await plcInfo.PlcAdapter.ReadUint32sAsync(block.StartAddr, block.Len);

                                            if (uint32s.IsSuccess)
                                            {
                                                for (int i = 0; i < addrs.Length; i++)
                                                {
                                                    // 更新缓存
                                                    _registValueCache.AddOrUpdate(addrs[i], (key) => uint32s.Data[i], (key, oldvalue) => uint32s.Data[i]);
                                                    // 释放block
                                                    if (keys.Contains(addrs[i]))
                                                    {
                                                        var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                                        if (exist)
                                                        {
                                                            tcs.SetResult(uint32s.Data[i]);
                                                            _registValueWaiter.TryRemove(addrs[i], out tcs);
                                                        }
                                                    }
                                                }
                                            }
                                            break;

                                        case ValueType.int64:
                                            var int64s = await plcInfo.PlcAdapter.ReadInt64sAsync(block.StartAddr, block.Len);

                                            if (int64s.IsSuccess)
                                            {
                                                for (int i = 0; i < addrs.Length; i++)
                                                {
                                                    // 更新缓存
                                                    _registValueCache.AddOrUpdate(addrs[i], (key) => int64s.Data[i], (key, oldvalue) => int64s.Data[i]);
                                                    // 释放block
                                                    if (keys.Contains(addrs[i]))
                                                    {
                                                        var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                                        if (exist)
                                                        {
                                                            tcs.SetResult(int64s.Data[i]);
                                                            _registValueWaiter.TryRemove(addrs[i], out tcs);
                                                        }
                                                    }
                                                }
                                            }

                                            break;

                                        case ValueType.uint64:

                                            var uint64s = await plcInfo.PlcAdapter.ReadUint64sAsync(block.StartAddr, block.Len);

                                            if (uint64s.IsSuccess)
                                            {
                                                for (int i = 0; i < addrs.Length; i++)
                                                {
                                                    // 更新缓存
                                                    _registValueCache.AddOrUpdate(addrs[i], (key) => uint64s.Data[i], (key, oldvalue) => uint64s.Data[i]);
                                                    // 释放block
                                                    if (keys.Contains(addrs[i]))
                                                    {
                                                        var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                                        if (exist)
                                                        {
                                                            tcs.SetResult(uint64s.Data[i]);
                                                            _registValueWaiter.TryRemove(addrs[i], out tcs);
                                                        }
                                                    }
                                                }
                                            }

                                            break;

                                        case ValueType.@float:

                                            var floats = await plcInfo.PlcAdapter.ReadFloatsAsync(block.StartAddr, block.Len);

                                            if (floats.IsSuccess)
                                            {
                                                for (int i = 0; i < addrs.Length; i++)
                                                {
                                                    // 更新缓存
                                                    _registValueCache.AddOrUpdate(addrs[i], (key) => floats.Data[i], (key, oldvalue) => floats.Data[i]);
                                                    // 释放block
                                                    if (keys.Contains(addrs[i]))
                                                    {
                                                        var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                                        if (exist)
                                                        {
                                                            tcs.SetResult(floats.Data[i]);
                                                            _registValueWaiter.TryRemove(addrs[i], out tcs);
                                                        }
                                                    }
                                                }
                                            }

                                            break;

                                        case ValueType.@double:

                                            var doubles = await plcInfo.PlcAdapter.ReadDoublesAsync(block.StartAddr, block.Len);

                                            if (doubles.IsSuccess)
                                            {
                                                for (int i = 0; i < addrs.Length; i++)
                                                {
                                                    // 更新缓存
                                                    _registValueCache.AddOrUpdate(addrs[i], (key) => doubles.Data[i], (key, oldvalue) => doubles.Data[i]);
                                                    // 释放block
                                                    if (keys.Contains(addrs[i]))
                                                    {
                                                        var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                                        if (exist)
                                                        {
                                                            tcs.SetResult(doubles.Data[i]);
                                                            _registValueWaiter.TryRemove(addrs[i], out tcs);
                                                        }
                                                    }
                                                }
                                            }

                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    for (int i = 0; i < addrs.Length; i++)
                                    {
                                        // 更新缓存
                                        _registValueCache.AddOrUpdate(addrs[i], (key) => null, (key, oldvalue) => null);
                                        // 释放block
                                        if (keys.Contains(addrs[i]))
                                        {
                                            var exist = _registValueWaiter.TryGetValue(addrs[i], out var tcs);
                                            if (exist)
                                            {
                                                tcs.SetResult(null);
                                                _registValueWaiter.TryRemove(addrs[i], out tcs);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }

        //private string GetAddrByName(string name)
        //{
        //    _plcInfos.ForEach(plcInfo =>
        //    {
        //        if(plcInfo.)
        //    });
        //}

        public OperationResult<bool> ReadBoolByBlock(string name)
        {
            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
            while (!_registValueWaiter.TryAdd(name, taskCompletionSource))
            {
                Thread.Sleep(30);
            }
            return new OperationResult<bool>()
            {
                Data = System.Convert.ToBoolean(taskCompletionSource.Task.GetAwaiter().GetResult())
            };
        }

        public async Task<OperationResult<bool>> ReadBoolByBlockAsync(string name)
        {
            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
            while (!_registValueWaiter.TryAdd(name, taskCompletionSource))
            {
                await Task.Delay(30).ConfigureAwait(false);
            }
            return new OperationResult<bool>()
            {
                Data = System.Convert.ToBoolean(taskCompletionSource.Task.GetAwaiter().GetResult())
            };
        }

        public OperationResult<bool> ReadBoolFromCache(string name)
        {
            throw new NotImplementedException();
        }

        public OperationResult<bool[]> ReadBoolsByBlock(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<bool[]>> ReadBoolsByBlockAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<bool[]> ReadBoolsFromCache(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<double> ReadDoubleByBlock(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<double>> ReadDoubleByBlockAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<double> ReadDoubleFromCache(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, double value)[]> ReadDoublesByBlock(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<(string addr, double value)[]>> ReadDoublesByBlockAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, double value)[]> ReadDoublesFromCache(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<float> ReadFloatByBlock(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<float>> ReadFloatByBlockAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<float> ReadFloatFromCache(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, float value)[]> ReadFloatsByBlock(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<(string addr, float value)[]>> ReadFloatsByBlockAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, float value)[]> ReadFloatsFromCache(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<short> ReadInt16ByBlock(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<short>> ReadInt16ByBlockAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<short> ReadInt16FromCache(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, short value)[]> ReadInt16sByBlock(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<(string addr, short value)[]>> ReadInt16sByBlockAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, short value)[]> ReadInt16sFromCache(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<int> ReadInt32ByBlock(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<int>> ReadInt32ByBlockAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<int> ReadInt32FromCache(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, int value)[]> ReadInt32sByBlock(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<(string addr, int value)[]>> ReadInt32sByBlockAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, int value)[]> ReadInt32sFromCache(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<long> ReadInt64ByBlock(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<long>> ReadInt64ByBlockAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<long> ReadInt64FromCache(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, long value)[]> ReadInt64sByBlock(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<(string addr, long value)[]>> ReadInt64sByBlockAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, long value)[]> ReadInt64sFromCache(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<byte> ReadInt8ByBlock(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<byte>> ReadInt8ByBlockAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<byte> ReadInt8FromCache(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, sbyte value)[]> ReadInt8sByBlock(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<(string addr, sbyte value)[]>> ReadInt8sByBlockAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, sbyte value)[]> ReadInt8sFromCache(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<ushort> ReadUint16ByBlock(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<ushort>> ReadUint16ByBlockAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<ushort> ReadUint16FromCache(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, ushort value)[]> ReadUint16sByBlock(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<(string addr, ushort value)[]>> ReadUint16sByBlockAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, ushort value)[]> ReadUint16sFromCache(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<uint> ReadUint32ByBlock(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<uint>> ReadUint32ByBlockAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<uint> ReadUint32FromCache(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, uint value)[]> ReadUint32sByBlock(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<(string addr, uint value)[]>> ReadUint32sByBlockAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, uint value)[]> ReadUint32sFromCache(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<ulong> ReadUint64ByBlock(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<ulong>> ReadUint64ByBlockAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<ulong> ReadUint64FromCache(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, ulong value)[]> ReadUint64sByBlock(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<(string addr, ulong value)[]>> ReadUint64sByBlockAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, ulong value)[]> ReadUint64sFromCache(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<byte> ReadUint8ByBlock(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<byte>> ReadUint8ByBlockAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<byte> ReadUint8FromCache(string addr)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, byte value)[]> ReadUint8sByBlock(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<(string addr, byte value)[]>> ReadUint8sByBlockAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public OperationResult<(string addr, byte value)[]> ReadUint8sFromCache(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
        }

        public void Terminate()
        {
        }
    }
}