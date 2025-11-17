using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.ObjectModel
{
    internal interface IDeviceManager
    {
        void AddDevice(IDevice device);
        void RemoveDevice(IDevice device);
        IDevice GetDeviceById(string deviceId);
    }
}
