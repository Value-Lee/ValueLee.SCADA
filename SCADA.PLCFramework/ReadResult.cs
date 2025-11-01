using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    internal readonly struct ReadResult
    {
        public long Timestamp { get; }

        public ReadResult()
        {
            Stopwatch.GetTimestamp();
            
        }

        

    }
}
