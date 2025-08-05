using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValueLee.ObjectModel
{
    public class DeviceBase : IDevice
    {
        public DeviceBase(string module, string name)
        {
            Module = module;
            Name = name;
        }

        public string Id => Module + "." + Name;
        public string Module { get; }

        public string Name { get; }

        public void Execute(string id, string methodName, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Execute(string module, string name, string methodName, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Monitor()
        {
            throw new NotImplementedException();
        }

        public void Register(string id, string methodName, ScriptMethodDelegate @delegate)
        {
            throw new NotImplementedException();
        }

        public void Register(string module, string name, string methodName, ScriptMethodDelegate @delegate)
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