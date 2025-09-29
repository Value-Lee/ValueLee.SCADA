using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.HardwareUnits.VacuumGauges
{
    public interface IVacuumGauge
    {
        double Feedback { get; set; }
    }
}
