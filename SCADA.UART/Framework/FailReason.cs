using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.UART.Framework
{
    public enum FailReason
    {
        Processing,
        ResponseTimeout,
        ResponseValidatorFailed,
        CancelledForBreakdown,
        HandlerException,
        HandlerSuccess
    }
}
