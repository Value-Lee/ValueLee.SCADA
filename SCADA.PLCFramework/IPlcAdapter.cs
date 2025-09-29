using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    public interface IPlcAdapter
    {
        byte? ReadByte(string addr);
        byte?[] ReadBytes(string addr, int count);
        byte?[] ReadBytes(params string[] addr);
    }
}
