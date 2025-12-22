using System;
using System.Collections.Generic;
using System.Text;

namespace SCADA.PLCFramework
{
    internal class PlcBoolSnapshot
    {
        public PlcBoolSnapshot()
        {
            
        }

        public long Timestamp { get; set; }
        public bool[] Values { get; set;  }
    }
}
