using MoreLinq;
using SCADA.Common;
using SCADA.ObjectModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
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
    public class PlcAdapterWrapper : DeviceBase, IDevice, IConnection, IPlcAccessor
    {
        private long _writeIdGenerator = long.MinValue;
        private readonly IPlcAdapter _plcAdapter;
        private readonly PlcInfo _plcInfo;
        private readonly ConcurrentQueue<WriteData> _writeDatasAoQueue;
        private readonly ConcurrentQueue<WriteData> _writeDatasDoQueue;
        private readonly ConcurrentDictionary<long, bool> _writeIdsSet;
        private int _aiSnapshotCount = 10;
        private int _aiSnapshotIndex = 0;
        private PlcByteSnapshot[] _aiValuesSnapshots;
        private PlcByteSnapshot _aoValuesSnapshot;
        private volatile PlcByteSnapshot _currentAIValuesSnapshot;
        private PlcByteSnapshot _currentaoValuesSnapshot;
        private volatile PlcBoolSnapshot _currentDIValuesSnapshot;
        private PlcBoolSnapshot _currentdoValuesSnapshot;
        private readonly int _diSnapshotCount = 10;
        private int _diSnapshotIndex = 0;
        private PlcBoolSnapshot[] _diValuesSnapshots;
        private PlcBoolSnapshot _doValuesSnapshot;
        private readonly HashSet<string> _toWriteDos = new HashSet<string>();
        public long WriteIdGenerator => Interlocked.Increment(ref _writeIdGenerator);
        public PlcAdapterWrapper(PlcInfo plcInfo) : base(plcInfo.Module, plcInfo.Name)
        {
            _writeDatasDoQueue = new ConcurrentQueue<WriteData>();
            _writeDatasAoQueue = new ConcurrentQueue<WriteData>();

            _writeIdsSet = new ConcurrentDictionary<long, bool>();

            _diValuesSnapshots = new PlcBoolSnapshot[_diSnapshotCount];
            _aiValuesSnapshots = new PlcByteSnapshot[_aiSnapshotCount];

            _plcInfo = plcInfo;
            _plcAdapter = Assembly.LoadFrom(plcInfo.Assembly)
                .GetType(plcInfo.Class)
                .GetConstructor([typeof(string)])
                .Invoke([plcInfo.Address]) as IPlcAdapter;

            // 启动定时读写PLC缓存到上位机线程
            Thread poller = new(new ThreadStart(PollingWorker))
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            poller.Start();
        }

        #region RW

        private RegistItem GetRegistItem(string name)
        {
            if (_plcInfo.DIs.TryGetValue(name, out RegistItem value1))
            {
                return value1;
            }
            if (_plcInfo.DOs.TryGetValue(name, out RegistItem value))
            {
                return value;
            }
            if (_plcInfo.AIs.TryGetValue(name, out RegistItem value2))
            {
                return value2;
            }
            if (_plcInfo.AOs.TryGetValue(name, out RegistItem value3))
            {
                return value3;
            }
            return null;
        }


        #region Read
        private void Read(string name, out long timestamp, out object value)
        {
            timestamp = 0;
            value = null;
            var item = GetRegistItem(name);
            var block = _plcInfo.Blocks.GetValueOrDefault(item.BlockId);
            if (block.Type == "di")
            {
                timestamp = _currentDIValuesSnapshot.Timestamp;
                value = _currentDIValuesSnapshot.Values[int.Parse(item.Index)];
            }
            else if (block.Type == "do")
            {
                timestamp = _currentdoValuesSnapshot.Timestamp;
                value = _currentdoValuesSnapshot.Values[int.Parse(item.Index)];
            }
            else if (block.Type == "ai")
            {
                timestamp = _currentAIValuesSnapshot.Timestamp;
                int byteIndex = -1;
                int bitIndex = -1;
                int dotIndex = item.Index.IndexOf('.');
                if (dotIndex != -1)
                {
                    ReadOnlySpan<char> chars = item.Index;
                    byteIndex = int.Parse(chars.Slice(0, dotIndex));
                    bitIndex = int.Parse(chars.Slice(dotIndex, item.Index.Length - dotIndex));
                }
                else
                {
                    byteIndex = int.Parse(item.Index);
                }

                if (item.Type.ToString().StartsWith("string"))
                {

                }
                else
                {
                    switch (item.Type)
                    {
                        case ValueType.@bool:
                            break;
                        case ValueType.int8:
                            break;
                        case ValueType.int16:
                            break;
                        case ValueType.int32:
                            break;
                        case ValueType.int64:
                            break;
                        case ValueType.uint8:
                            break;
                        case ValueType.uint16:
                            break;
                        case ValueType.uint32:
                            break;
                        case ValueType.uint64:
                            break;
                        case ValueType.@float:
                            break;
                        case ValueType.@double:
                            break;
                    }
                }
            }
            else if (block.Type == "ao")
            {

            }
        }

        public ReadResult<T> Read<T>(string name)
        {
            Read(name, out long timestamp, out object value);
            return new ReadResult<T>(timestamp, (T)value);
        }

        public ReadResult<T1, T2> Read<T1, T2>(string name1, string name2)
        {
            var result = new ReadResult<T1, T2>();
            Read(name1, out long timestamp1, out object value1);
            Read(name2, out long timestamp2, out object value2);
            result.Timestamp = timestamp2;
            result.Value1 = (T1)value1;
            result.Value2 = (T2)value2;
            return result;
        }

        public ReadResult<T1, T2, T3> Read<T1, T2, T3>(string name1, string name2, string name3)
        {
            var result = new ReadResult<T1, T2, T3>();
            Read(name1, out long timestamp1, out object value1);
            Read(name2, out long timestamp2, out object value2);
            Read(name3, out long timestamp3, out object value3);
            result.Timestamp = timestamp3;
            result.Value1 = (T1)value1;
            result.Value2 = (T2)value2;
            result.Value3 = (T3)value3;
            return result;
        }

        public ReadResult<T1, T2, T3, T4> Read<T1, T2, T3, T4>(string name1, string name2, string name3, string name4)
        {
            var result = new ReadResult<T1, T2, T3, T4>();
            Read(name1, out long timestamp1, out object value1);
            Read(name2, out long timestamp2, out object value2);
            Read(name3, out long timestamp3, out object value3);
            Read(name4, out long timestamp4, out object value4);
            result.Timestamp = timestamp4;
            result.Value1 = (T1)value1;
            result.Value2 = (T2)value2;
            result.Value3 = (T3)value3;
            result.Value4 = (T4)value4;
            return result;
        }

        public ReadResult<T1, T2, T3, T4, T5> Read<T1, T2, T3, T4, T5>(string name1, string name2, string name3, string name4, string name5)
        {
            var result = new ReadResult<T1, T2, T3, T4, T5>();
            Read(name1, out long timestamp1, out object value1);
            Read(name2, out long timestamp2, out object value2);
            Read(name3, out long timestamp3, out object value3);
            Read(name4, out long timestamp4, out object value4);
            Read(name5, out long timestamp5, out object value5);
            result.Timestamp = timestamp5;
            result.Value1 = (T1)value1;
            result.Value2 = (T2)value2;
            result.Value3 = (T3)value3;
            result.Value4 = (T4)value4;
            result.Value5 = (T5)value5;
            return result;
        }

        public ReadResult<T1, T2, T3, T4, T5, T6> Read<T1, T2, T3, T4, T5, T6>(string name1, string name2, string name3, string name4, string name5, string name6)
        {
            var result = new ReadResult<T1, T2, T3, T4, T5, T6>();
            Read(name1, out long timestamp1, out object value1);
            Read(name2, out long timestamp2, out object value2);
            Read(name3, out long timestamp3, out object value3);
            Read(name4, out long timestamp4, out object value4);
            Read(name5, out long timestamp5, out object value5);
            Read(name6, out long timestamp6, out object value6);
            result.Timestamp = timestamp6;
            result.Value1 = (T1)value1;
            result.Value2 = (T2)value2;
            result.Value3 = (T3)value3;
            result.Value4 = (T4)value4;
            result.Value5 = (T5)value5;
            result.Value6 = (T6)value6;
            return result;
        }

        public ReadResult Read(params string[] names)
        {
            var result = new ReadResult();
            result.Values = new object[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                Read(names[i], out long timestamp, out object value);
                result.Timestamp = timestamp;
                result.Values[i] = value;
            }
            return result;
        }

        #endregion

        #region Write
        public long Write<T>(string name, T value)
        {
            var id = WriteIdGenerator;
            _writeIdsSet.TryAdd(id, false);
            _writeDatasDoQueue.Enqueue(new WriteData)
            return id;

        }

        public long Write<T1,T2>(string name1, T1 value1, string name2, T2 value2)
        {
            
        }

        public long Write<T1,T2,T3>(string name1, T1 value1, string name2, T2 value2, string name3, T3 value3)
        {
           
        }

        public long Write<T1,T2,T3,T4>(string name1, T1 value1, string name2, T2 value2, string name3, T3 value3, string name4, T4 value4)
        {
       
        }

        public long Write<T1, T2, T3, T4,T5>(string name1, T1 value1, string name2, T2 value2, string name3, T3 value3, string name4, T4 value4, string name5, T5 value5)
        {
           
        }

        public long Write<T1, T2, T3, T4, T5,T6>(string name1, T1 value1, string name2, T2 value2, string name3, T3 value3, string name4, T4 value4, string name5, T5 value5, string name6, T6 value6)
        {
           
        }

        public long Write(params (string name, object value)[] nameValues)
        {
            
        } 
        #endregion

        #endregion RW

        public bool WriteCompleted(long id)
        {
            throw new NotImplementedException();
        }
        private void PollingWorker()
        {
            while (true)
            {
                foreach (var block in _plcInfo.Blocks)
                {
                    // polling di & ai

                    if (block.Value.Type == "di" && block.Value.Polling == true)
                    {
                        var bools = _plcAdapter.ReadDIs(block.Value.StartAddr, block.Value.Len);
                        var nextIndex = (_diSnapshotIndex++) % _diSnapshotCount;
                        _diValuesSnapshots[nextIndex].Timestamp = DateTime.Now.Ticks;
                        bools.CopyTo(_diValuesSnapshots[nextIndex].Values);
                        Interlocked.Exchange(ref _currentDIValuesSnapshot, _diValuesSnapshots[nextIndex]);
                    }

                    if (block.Value.Type == "ai" && block.Value.Polling == true)
                    {
                        var bytes = _plcAdapter.ReadAIs(block.Value.StartAddr, block.Value.Len);
                        var nextIndex = (_aiSnapshotIndex++) % _aiSnapshotCount;
                        _diValuesSnapshots[nextIndex].Timestamp = DateTime.Now.Ticks;
                        bytes.CopyTo(_aiValuesSnapshots[nextIndex].Values);
                        Interlocked.Exchange(ref _currentAIValuesSnapshot, _aiValuesSnapshots[nextIndex]);
                    }

                    // write do & ao
                    if (block.Value.Type == "do" && block.Value.Polling == true)
                    {
                        _toWriteDos.Clear();
                        while (_writeDatasDoQueue.TryPeek(out var writeData))
                        {
                            bool isPresent = false;
                            foreach (var item in writeData.NameValuePairs)
                            {
                                if (_toWriteDos.Any(x => x == item.name))
                                {
                                    isPresent = true;
                                    break;
                                }
                            }
                            if (isPresent)
                            {
                                break;
                            }
                            else
                            {
                                writeData.NameValuePairs.ForEach(x => _toWriteDos.Add(x.name));
                                _writeDatasDoQueue.TryDequeue(out var _);
                            }
                        }
                        _currentdoValuesSnapshot.Values.AsSpan().CopyTo(_doValuesSnapshot.Values);
                        // 把_toWriteDos准换成bool或字节写进_doValuesSnapshot.Values
                        _plcAdapter.WriteDOs(block.Value.StartAddr, block.Value);
                        // 更新_doValuesSnapshot时间戳
                        _doValuesSnapshot.Timestamp = DateTime.Now.Ticks;
                        Interlocked.Exchange(ref _currentdoValuesSnapshot, _doValuesSnapshot);
                    }

                    if (block.Value.Type == "ao" && block.Value.Polling == true)
                    {
                        while (_writeDatasAoQueue.TryPeek(out var writeData))
                        {
                            bool isPresent = false;
                            foreach (var item in writeData.NameValuePairs)
                            {
                                if (_toWriteDos.Any(x => x == item.name))
                                {
                                    isPresent = true;
                                    break;
                                }
                            }
                            if (isPresent)
                            {
                                break;
                            }
                            else
                            {
                                writeData.NameValuePairs.ForEach(x => _toWriteDos.Add(x.name));
                                _writeDatasAoQueue.TryDequeue(out var _);
                            }
                        }
                        _currentaoValuesSnapshot.Values.AsSpan().CopyTo(_aoValuesSnapshot.Values);
                        // 把_toWriteDos准换成bool或字节写进_aoValuesSnapshot.Values
                        _plcAdapter.WriteAOs(block.Value.StartAddr, block.Value);
                        // 更新_aoValuesSnapshot时间戳
                        _aoValuesSnapshot.Timestamp = DateTime.Now.Ticks;
                        Interlocked.Exchange(ref _currentaoValuesSnapshot, _aoValuesSnapshot);
                    }
                }

                Thread.Sleep(20);
            }
        }
    }
}