using SCADA.Common;
using SCADA.PLCFramework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PlcAdapters.Beckhoff
{
    public class StructVaribleAdsAdapter : IPlcAdapter
    {
        public string Address { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public OperationResult Connect()
        {
            throw new NotImplementedException();
        }

        public double Convert2Double(IList<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public double Convert2Double(IList<byte> bytes, int startIndex)
        {
            throw new NotImplementedException();
        }

        public float Convert2Float(IList<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public float Convert2Float(IList<byte> bytes, int startIndex)
        {
            throw new NotImplementedException();
        }

        public short Convert2Int16(IList<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public short Convert2Int16(IList<byte> bytes, int startIndex)
        {
            throw new NotImplementedException();
        }

        public int Convert2Int32(IList<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public int Convert2Int32(IList<byte> bytes, int startIndex)
        {
            throw new NotImplementedException();
        }

        public long Convert2Int64(IList<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public long Convert2Int64(IList<byte> bytes, int startIndex)
        {
            throw new NotImplementedException();
        }

        public string Convert2String(IList<byte> bytes, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public string Convert2String(IList<byte> bytes, int startIndex, int length, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public ushort Convert2UInt16(IList<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public ushort Convert2UInt16(IList<byte> bytes, int startIndex)
        {
            throw new NotImplementedException();
        }

        public uint Convert2UInt32(IList<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public uint Convert2UInt32(IList<byte> bytes, int startIndex)
        {
            throw new NotImplementedException();
        }

        public ulong Convert2UInt64(IList<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public ulong Convert2UInt64(IList<byte> bytes, int startIndex)
        {
            throw new NotImplementedException();
        }

        public OperationResult Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public OperationResult<bool> IsConnected()
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> ReadAIs(string startAddr, int length)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> ReadAOs(string startAddr, int length)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> ReadDIs(string startAddr, int length)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> ReadDOs(string startAddr, int length)
        {
            throw new NotImplementedException();
        }

        public void WriteAOs(string startAddr, IList<byte> values)
        {
            throw new NotImplementedException();
        }

        public void WriteDOs(string startAddr, IList<bool> values)
        {
            throw new NotImplementedException();
        }
    }
}
