using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.UART.Framework
{
    public class RequestResult
    {
        public FailReason FailReason { get; set; }

        public string FailDetail { get; set; }

        public object Data { get; set; }

        public override bool Equals(object obj)
        {
            return obj is RequestResult result &&
                   FailReason == result.FailReason &&
                   FailDetail == result.FailDetail &&
                   EqualityComparer<object>.Default.Equals(Data, result.Data);
        }

        public override int GetHashCode()
        {
            return FailReason.GetHashCode() ^ FailDetail.GetHashCode();
        }
    }
}