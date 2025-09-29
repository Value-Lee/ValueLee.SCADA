using SCADA.Common;
using SCADA.UART.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.Nuart.Sample
{
    internal class Chiller : ITimerDemander
    {
        private SerialPortClient<char> _serialPort;

        public Chiller()
        {
            _serialPort = new SerialPortClient<char>(new Chiller_Filter(),
                new Chiller_Validator(),
                new CommunicationOptions("COM1", 9600, System.IO.Ports.Parity.None, System.IO.Ports.StopBits.One));

            _serialPort.BreakdownOcurred += _serialPort_BreakdownOcurred;
        }

        public bool IsConnected { get; set; }

        private void _serialPort_BreakdownOcurred()
        {
            IsConnected = false;
        }

        int? id;
        public void Monitor()
        {
            if(id.HasValue == false)
            {
                id = ReadTemp();
            }
            var ret = IsOverReadTime(id.Value);
            if (ret.isOver)
            {
                Console.WriteLine($"Temp: {ret.temp}");
                id = null;
            }
        }

        public int ReadTemp()
        {
            var chars = "SET_TEMP;25\r\n".ToCharArray();
            var id = _serialPort.Request(chars, "ReadTemp", PriorityLevel.Lower, 0, 200);
            return id;
        }

        public async Task<double> WaitReadTime(int id)
        {
            while (true)
            {
                if(_serialPort.IsRequestProcessed(id, out RequestResult result))
                {
                    if(result.FailReason == FailReason.HandlerSuccess)
                    {
                        double temp = Convert.ToDouble(result.Data);
                        return temp;
                    }
                    else
                    {

                    }
                }
                else
                {
                   await  Task.Delay(30);
                }
            }
            
        }

        public (bool isOver, double temp) IsOverReadTime(int id)
        {
            if (_serialPort.IsRequestProcessed(id, out RequestResult result))
            {
                if (result.FailReason == FailReason.HandlerSuccess)
                {
                    double temp = Convert.ToDouble(result.Data);
                    return (true,temp);
                }
                else
                {
                    return (true, 0);
                }
            }
            return (false,0d);
        }
    }
}
