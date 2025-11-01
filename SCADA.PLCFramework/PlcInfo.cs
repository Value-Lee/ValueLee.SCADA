using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    public class PlcInfo
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public Block[] Blocks { get; set; }
        public string Class { get; set; }
        public string Assembly { get; set; }
        public Dictionary<string, RegistItem> DIs { get; set; }
        public Dictionary<string, RegistItem> DOs { get; set; }
        public Dictionary<string, RegistItem> AIs { get; set; }
        public Dictionary<string, RegistItem> AOs { get; set; }
    }
}
