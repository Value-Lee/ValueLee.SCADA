using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.UART.Framework
{
    public interface IValidator<TCharOrByte> where TCharOrByte : struct, IConvertible, IComparable
    {
        bool IsValid(IList<TCharOrByte> response);
    }
}
