using System;
using System.Collections.Generic;
using System.Text;

namespace SCADA.TimerFSM.Interfaces
{
    public interface ITaktOptimizer
    {
        ITaktOptimizer FineTuneCheckInterval(TimeSpan timeSpan);
    }
}
