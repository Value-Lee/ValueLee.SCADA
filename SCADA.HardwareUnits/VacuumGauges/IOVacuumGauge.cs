using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.HardwareUnits.VacuumGauges
{
    public class IOVacuumGauge : IVacuumGauge
    {
        public double Feedback { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string AIName { get; set; }


    }
}
