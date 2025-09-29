using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.UART.Framework
{
    public interface IReceiveFilter<TCharOrByte> where TCharOrByte : struct, IConvertible, IComparable
    {
        bool IsComplete(IList<TCharOrByte> recvBuff, IList<TCharOrByte> reqContent);
    }
}
