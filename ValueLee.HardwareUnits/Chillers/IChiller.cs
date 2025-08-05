using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValueLee.Common;

namespace ValueLee.HardwareUnits.Chillers
{
    public interface IChiller
    {
        OperationResult TurnOnCH1();
        OperationResult TurnOnCH2();
        OperationResult TurnOffCH1();
        OperationResult TurnOffCH2();

        OperationResult SetTemperatureCH1(double temperature);
        OperationResult SetTemperatureCH2(double temperature);
    }
}
