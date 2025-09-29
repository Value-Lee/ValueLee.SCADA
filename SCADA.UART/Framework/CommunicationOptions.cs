using System;
using System.IO.Ports;

namespace SCADA.UART.Framework
{
    public class CommunicationOptions
    {
        public CommunicationOptions(string portName, int baudRate, Parity parity, StopBits stopBits, int dataBits, bool rtsEnable, Handshake handshake)
        {
            if (string.IsNullOrWhiteSpace(portName)) throw new ArgumentNullException(nameof(portName));
            if (baudRate <= 0) throw new ArgumentOutOfRangeException(nameof(baudRate), "BaudRate must be positive.");
            if (dataBits < 5 || dataBits > 8) throw new ArgumentOutOfRangeException(nameof(dataBits), "DataBits must be between 5 and 8.");
            PortName = portName;
            BaudRate = baudRate;
            Parity = parity;
            StopBits = stopBits;
            DataBits = dataBits;
            RtsEnable = rtsEnable;
            Handshake = handshake;
        }

        public CommunicationOptions(string portName, int baudRate, Parity parity, StopBits stopBits, int dataBits)
            : this(portName, baudRate, parity, stopBits, dataBits, false, Handshake.None) { }

        public CommunicationOptions(string portName, int baudRate, Parity parity, StopBits stopBits)
            : this(portName, baudRate, parity, stopBits, 8) { }

        public int BaudRate { get; }
        public int DataBits { get; }
        public Handshake Handshake { get; }
        public Parity Parity { get; }
        public string PortName { get; }
        public bool RtsEnable { get; }
        public StopBits StopBits { get; }
    }
}