using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    public class RegistItem
    {
        public RegistItem(string name, string addr, ValueType type, string display, string desc)
        {
            Name = name;
            Addr = addr;
            Type = type;
            Desc = desc;
            Display = display;
        }

        public string Name { get; }
        public string Addr { get; }
        public string Display { get; }
        public ValueType Type { get; }
        public string Desc { get; }
    }
}