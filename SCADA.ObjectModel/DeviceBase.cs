using SCADA.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.ObjectModel
{
    // 1. 数据采集功能 2. AlarmEvent注册抛出功能 3.脚本功能
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

        string IModular.Module => throw new NotImplementedException();

        string IModular.Name => throw new NotImplementedException();

        string IModular.Id => throw new NotImplementedException();

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

        void IScript.Execute(string id, string methodName, params object[] args)
        {
            throw new NotImplementedException();
        }

        void IScript.Execute(string module, string name, string methodName, params object[] args)
        {
            throw new NotImplementedException();
        }

        void IDevice.Initialize()
        {
            throw new NotImplementedException();
        }

        void ITimerDemander.Monitor()
        {
            throw new NotImplementedException();
        }

        void IScript.Register(string id, string methodName, ScriptMethodDelegate @delegate)
        {
            throw new NotImplementedException();
        }

        void IScript.Register(string module, string name, string methodName, ScriptMethodDelegate @delegate)
        {
            throw new NotImplementedException();
        }

        void IDataLogger.Register(string id, Func<object> getter)
        {
            throw new NotImplementedException();
        }

        void IDevice.Reset()
        {
            throw new NotImplementedException();
        }

        void IDevice.Terminate()
        {
            throw new NotImplementedException();
        }
    }
}