using SCADA.Common;
using SCADA.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    public interface IPlcAdapter : IConnection, IDisposable
    {
        #region Read & Write
        ReadOnlySpan<byte> ReadAIs(string startAddr, int length);
        ReadOnlySpan<byte> ReadAOs(string startAddr, int length);
        ReadOnlySpan<bool> ReadDIs(string startAddr, int length);
        ReadOnlySpan<bool> ReadDOs(string startAddr, int length);
        void WriteAOs(string startAddr, IList<byte> values);
        void WriteDOs(string startAddr, IList<bool> values);
        #endregion Read & Write

        #region Data Type Conversion Function

        double Convert2Double(IList<byte> bytes);

        double Convert2Double(IList<byte> bytes, int startIndex);

        float Convert2Float(IList<byte> bytes);

        float Convert2Float(IList<byte> bytes, int startIndex);

        short Convert2Int16(IList<byte> bytes);

        short Convert2Int16(IList<byte> bytes, int startIndex);

        int Convert2Int32(IList<byte> bytes);

        int Convert2Int32(IList<byte> bytes, int startIndex);

        long Convert2Int64(IList<byte> bytes);

        long Convert2Int64(IList<byte> bytes, int startIndex);

        string Convert2String(IList<byte> bytes, Encoding encoding);

        string Convert2String(IList<byte> bytes, int startIndex, int length, Encoding encoding);

        ushort Convert2UInt16(IList<byte> bytes);

        ushort Convert2UInt16(IList<byte> bytes, int startIndex);

        uint Convert2UInt32(IList<byte> bytes);

        uint Convert2UInt32(IList<byte> bytes, int startIndex);

        ulong Convert2UInt64(IList<byte> bytes);

        ulong Convert2UInt64(IList<byte> bytes, int startIndex);

        #endregion Data Type Conversion Function
    }
}