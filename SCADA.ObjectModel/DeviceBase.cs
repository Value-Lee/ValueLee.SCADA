using SCADA.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.ObjectModel
{
    // 1. 数据采集功能 2. AlarmEvent注册抛出功能 3.脚本功能
    public class DeviceBase : IDevice,IModular
    {
        public DeviceBase(string module, string name)
        {
            Module = module;
            Name = name;
        }

        public string Id => Module + "." + Name;
        public string Module { get; }
        public string Name { get; }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Monitor()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}