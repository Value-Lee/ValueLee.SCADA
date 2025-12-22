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
using SCADA.Common;
using SCADA.ObjectModel;

namespace SCADA.PLCFramework
{
    public class PlcAccessor : IPlcAccessor, IDevice
    {
        private readonly ConcurrentDictionary<string, object> _registValueCache;
        private List<PlcInfo> _plcInfos;

        public PlcAccessor()
        {
            _registValueCache = new ConcurrentDictionary<string, object>();
            _plcInfos = new List<PlcInfo>();
        }

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
            var plcInfoCollection = JsonSerializer.Deserialize<List<PlcInfo>>(plcInfoFile);
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

        public void Reset()
        {
        }

        public void Terminate()
        {
        }
    }
}