using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nuart.Modbus.TestApp.SerialPortSettingChangedEvent;

namespace Nuart.Modbus.TestApp
{
    public class SerialPortSettingChangedEvent : PubSubEvent<SerialPortSettingChangedEventArgs>
    {
        public class SerialPortSettingChangedEventArgs
        {
            public string PortName { get; }
            public int BaudRate { get; }
            public Parity Parity { get; }
            public StopBits StopBits { get; }

            public SerialPortSettingChangedEventArgs(string portName, int baudRate, Parity parity, StopBits stopBits)
            {
                PortName = portName;
                BaudRate = baudRate;
                Parity = parity;
                StopBits = stopBits;
            }
        }
    }


}
