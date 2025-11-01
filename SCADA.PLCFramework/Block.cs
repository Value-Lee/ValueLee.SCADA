using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    // Block不需要完整的配全所有的PLC地址，只是作为整合零散地址的整理参考。
    public class Block
    {
        public string StartAddr { get; set; }
        public int Len { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Block other)
            {
                return StartAddr == other.StartAddr &&
                       Len == other.Len;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartAddr, Len);
        }
    }
}
