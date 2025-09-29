using SCADA.UART.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.Nuart.Sample
{
    internal class Chiller_Validator : IValidator<char>
    {
        public bool IsValid(IList<char> response)
        {
            return true;
        }
    }
}
