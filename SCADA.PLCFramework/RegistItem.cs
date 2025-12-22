using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    public class RegistItem
    {
        public RegistItem(string name, string index, ValueType type, string display, string desc, string blockId)
        {
            Name = name;
            Index = index;
            Type = type;
            Desc = desc;
            Display = display;
            BlockId = blockId;
        }

        public string Name { get; }
        public string Index { get; }
        public string Display { get; }
        public ValueType Type { get; }
        public string BlockId { get; }
        public string Desc { get; }
    }
}