using HslCommunication.Profinet.Melsec;
using HslCommunication.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework.Test
{
    public class MCPlcAdapter : IPlcAdapter
    {
        private MelsecMcNet _melsecMcNet;
        public MCPlcAdapter()
        {
            _melsecMcNet = new MelsecMcNet();
        }

        public void Connect(string ip, int port)
        {
            _melsecMcNet.IpAddress = ip;
            _melsecMcNet.Port = port;
            _melsecMcNet.ConnectServer();
        }

        public async Task ConnectAsync(string ip, int port)
        {
            _melsecMcNet.IpAddress = ip;
            _melsecMcNet.Port = port;
            await _melsecMcNet.ConnectServerAsync();
        }

        public bool ReadBool(string addr)
        {
            return _melsecMcNet.ReadBool(addr).Content;
        }

        public async Task<bool> ReadBoolAsync(string addr)
        {
            var v = await _melsecMcNet.ReadBoolAsync(addr);
            return v.Content;
        }

        public bool[] ReadBools(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<bool[]> ReadBoolsAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public double ReadDouble(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<double> ReadDoubleAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public (string addr, double value)[] ReadDoubles(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<(string addr, double value)[]> ReadDoublesAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public float ReadFloat(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<float> ReadFloatAsync(string addr)
        {
            throw new NotImplementedException();
        }

        public (string addr, float value)[] ReadFloats(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<(string addr, float value)[]> ReadFloatsAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public short ReadInt16(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<short> ReadInt16Async(string addr)
        {
            throw new NotImplementedException();
        }

        public (string addr, short value)[] ReadInt16s(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<(string addr, short value)[]> ReadInt16sAsync(int addr, int length)
        {
            throw new NotImplementedException();
        }

        public int ReadInt32(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<int> ReadInt32Async(string addr)
        {
            throw new NotImplementedException();
        }

        public (string addr, int value)[] ReadInt32s(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<(string addr, int value)[]> ReadInt32sAsync(int addr, int length)
        {
            throw new NotImplementedException();
        }

        public long ReadInt64(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<long> ReadInt64Async(string addr)
        {
            throw new NotImplementedException();
        }

        public (string addr, long value)[] ReadInt64s(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<(string addr, long value)[]> ReadInt64sAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public byte ReadInt8(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<byte> ReadInt8Async(string addr)
        {
            throw new NotImplementedException();
        }

        public (string addr, sbyte value)[] ReadInt8s(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<(string addr, sbyte value)[]> ReadInt8sAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public ushort ReadUint16(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<ushort> ReadUint16Async(string addr)
        {
            throw new NotImplementedException();
        }

        public (string addr, ushort value)[] ReadUint16s(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<(string addr, ushort value)[]> ReadUint16sAsync(int addr, int length)
        {
            throw new NotImplementedException();
        }

        public uint ReadUint32(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<uint> ReadUint32Async(string addr)
        {
            throw new NotImplementedException();
        }

        public (string addr, uint value)[] ReadUint32s(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<(string addr, uint value)[]> ReadUint32sAsync(int addr, int length)
        {
            throw new NotImplementedException();
        }

        public ulong ReadUint64(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<ulong> ReadUint64Async(string addr)
        {
            throw new NotImplementedException();
        }

        public (string addr, ulong value)[] ReadUint64s(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<(string addr, ulong value)[]> ReadUint64sAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public byte ReadUint8(string addr)
        {
            throw new NotImplementedException();
        }

        public Task<byte> ReadUint8Async(string addr)
        {
            throw new NotImplementedException();
        }

        public (string addr, byte value)[] ReadUint8s(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public Task<(string addr, byte value)[]> ReadUint8sAsync(string addr, int length)
        {
            throw new NotImplementedException();
        }

        public string[] ResolveAddrFormat(string addr, ValueType valueType, int length)
        {
            throw new NotImplementedException();
        }

        public void WriteBool(string addr, bool value)
        {
            throw new NotImplementedException();
        }

        public Task WriteBoolAsync(string addr, bool value)
        {
            throw new NotImplementedException();
        }

        public void WriteBools(string startAddr, IList<bool> values)
        {
            throw new NotImplementedException();
        }

        public Task WriteBoolsAsync(string startAddr, IList<bool> values)
        {
            throw new NotImplementedException();
        }

        public void WriteDouble(string addr, double value)
        {
            throw new NotImplementedException();
        }

        public Task WriteDoubleAsync(string addr, double value)
        {
            throw new NotImplementedException();
        }

        public void WriteDoubles(string startAddr, IList<double> values)
        {
            throw new NotImplementedException();
        }

        public Task WriteDoublesAsync(string startAddr, IList<double> values)
        {
            throw new NotImplementedException();
        }

        public void WriteFloat(string addr, float value)
        {
            throw new NotImplementedException();
        }

        public Task WriteFloatAsync(string addr, float value)
        {
            throw new NotImplementedException();
        }

        public void WriteFloats(string startAddr, IList<float> values)
        {
            throw new NotImplementedException();
        }

        public Task WriteFloatsAsync(string startAddr, IList<float> values)
        {
            throw new NotImplementedException();
        }

        public void WriteInt16(string addr, short value)
        {
            throw new NotImplementedException();
        }

        public Task WriteInt16Async(string addr, short value)
        {
            throw new NotImplementedException();
        }

        public void WriteInt16s(string startAddr, IList<short> values)
        {
            throw new NotImplementedException();
        }

        public Task WriteInt16sAsync(string startAddr, IList<short> values)
        {
            throw new NotImplementedException();
        }

        public void WriteInt32(string addr, int value)
        {
            throw new NotImplementedException();
        }

        public Task WriteInt32Async(string addr, int value)
        {
            throw new NotImplementedException();
        }

        public void WriteInt32s(string startAddr, IList<int> values)
        {
            throw new NotImplementedException();
        }

        public Task WriteInt32sAsync(string startAddr, IList<int> values)
        {
            throw new NotImplementedException();
        }

        public void WriteInt64(string addr, long value)
        {
            throw new NotImplementedException();
        }

        public Task WriteInt64Async(string addr, long value)
        {
            throw new NotImplementedException();
        }

        public void WriteInt64s(string startAddr, IList<long> values)
        {
            throw new NotImplementedException();
        }

        public Task WriteInt64sAsync(string startAddr, IList<long> values)
        {
            throw new NotImplementedException();
        }

        public void WriteInt8(string addr, sbyte value)
        {
            throw new NotImplementedException();
        }

        public Task WriteInt8Async(string addr, sbyte value)
        {
            throw new NotImplementedException();
        }

        public void WriteInt8s(string startAddr, IList<sbyte> values)
        {
            throw new NotImplementedException();
        }

        public Task WriteInt8sAsync(string startAddr, IList<sbyte> values)
        {
            throw new NotImplementedException();
        }

        public void WriteUint16(string addr, ushort value)
        {
            throw new NotImplementedException();
        }

        public Task WriteUint16Async(string addr, ushort value)
        {
            throw new NotImplementedException();
        }

        public void WriteUint16s(string startAddr, IList<ushort> values)
        {
            throw new NotImplementedException();
        }

        public Task WriteUint16sAsync(string startAddr, IList<ushort> values)
        {
            throw new NotImplementedException();
        }

        public void WriteUint32(string addr, uint value)
        {
            throw new NotImplementedException();
        }

        public Task WriteUint32Async(string addr, uint value)
        {
            throw new NotImplementedException();
        }

        public void WriteUint32s(string startAddr, IList<uint> values)
        {
            throw new NotImplementedException();
        }

        public Task WriteUint32sAsync(string startAddr, IList<uint> values)
        {
            throw new NotImplementedException();
        }

        public void WriteUint64(string addr, ulong value)
        {
            throw new NotImplementedException();
        }

        public Task WriteUint64Async(string addr, ulong value)
        {
            throw new NotImplementedException();
        }

        public void WriteUint64s(string startAddr, IList<ulong> values)
        {
            throw new NotImplementedException();
        }

        public Task WriteUint64sAsync(string startAddr, IList<ulong> values)
        {
            throw new NotImplementedException();
        }

        public void WriteUint8(string addr, byte value)
        {
            throw new NotImplementedException();
        }

        public Task WriteUint8Async(string addr, byte value)
        {
            throw new NotImplementedException();
        }

        public void WriteUint8s(string startAddr, IList<byte> values)
        {
            throw new NotImplementedException();
        }

        public Task WriteUint8sAsync(string startAddr, IList<byte> values)
        {
            throw new NotImplementedException();
        }
    }
}
