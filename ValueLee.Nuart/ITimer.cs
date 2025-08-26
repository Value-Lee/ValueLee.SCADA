using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValueLee.Nuart
{
    public interface ITimer
    {
        ValueLee.Common.PeriodicTimer PeriodicTimer { get; set; }

        void RecvData();
    }
}