using SCADA.UART.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.Nuart.Sample
{
    /*  CMD;Param1;Param2;...CRLF
     *  
     *  SET_TEMP;25\r\n       OK\r\n
     *  RED_TEMP?\r\n         20\r\n
     */
    internal class Chiller_Filter : IReceiveFilter<char>
    {
        public bool IsComplete(IList<char> recvBuff, IList<char> reqContent)
        {
            int len = recvBuff.Count;
            if (len < 3) return false;
            return recvBuff[len - 2] == '\r' && recvBuff[len - 1] == '\n';
        }
    }
}
