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
        #region Read & Write DI DO AI AO
        Task<bool[]> ReadDIsAsync(string startAddr, int length);
        Task<bool[]> ReadDOsAsync(string startAddr, int length);
        Task WriteDOsAsync(string startAddr, IList<bool> values);
        Task<byte[]> ReadAIsAsync(string startAddr, int length);
        Task<byte[]> ReadAOsAsync(string startAddr, int length);
        Task WriteDOsAsync(string startAddr, IList<byte> values);
        #endregion

        #region Data Type Conversion Function
        short Convert2Int16(IList<byte> bytes);
        short Convert2Int16(IList<byte> bytes, int startIndex );

        ushort Convert2UInt16(IList<byte> bytes);
        ushort Convert2UInt16(IList<byte> bytes, int startIndex);


        int Convert2Int32(IList<byte> bytes);
        int Convert2Int32(IList<byte> bytes, int startIndex);

        uint Convert2UInt32(IList<byte> bytes);
        uint Convert2UInt32(IList<byte> bytes, int startIndex);

        long Convert2Int64(IList<byte> bytes);
        long Convert2Int64(IList<byte> bytes, int startIndex);

        ulong Convert2UInt64(IList<byte> bytes);
        ulong Convert2UInt64(IList<byte> bytes, int startIndex);

        float Convert2Float(IList<byte> bytes);
        float Convert2Float(IList<byte> bytes, int startIndex);

        double Convert2Double(IList<byte> bytes);
        double Convert2Double(IList<byte> bytes, int startIndex);

        string Convert2String(IList<byte> bytes, Encoding encoding);
        string Convert2String(IList<byte> bytes, int startIndex, int length, Encoding encoding);
        #endregion
    }
}