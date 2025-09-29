using SCADA.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.HardwareUnits.Chillers
{
    public interface IChiller
    {
        int ChannelCount { get; }

        OperationResult TurnOn(int channel);

        OperationResult TurnOff(int channel);

        OperationResult SetTemperature(int channel, double temperature);
    }
}