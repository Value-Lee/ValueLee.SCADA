using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    public class Block
    {
        public string ID { get; set; }
        public string Type { get; set; }
        public bool Polling { get; set; }
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
