using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    public  class PlcAccessor:IPlcAccessor
    {
        private static PlcAccessor Instance { get; } = new PlcAccessor();
        private PlcAccessor() { }

        public  void Load(string settingFile)
        {

        }


        public  void Add()
        {

        }

        public  byte ReadByte(RWMode mode = RWMode.FromCache)
        {

        }
    }
}
