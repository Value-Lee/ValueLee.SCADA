using System;
using System.IO.Ports;
using ValueLee.Nuart;

namespace Nuart.RequestReplyModel
{
    public interface ISerialPort : IDisposable
    {
        event Action<SerialEventArgs<byte[]>> CompletedFrameReceived;

        event Action<SerialEventArgs<byte[]>> DataRead;

        event Action<SerialEventArgs<byte[]>> DataSent;

        int BaudRate { get; }

        int DataBits { get; }

        Handshake Handshake { get; }

        int LastCompletedFrameResolvedTime { get; }

        Parity Parity { get; }

        string PortName { get; }

        int RecvBuffLength { get; }

        bool RtsEnable { get; }

        StopBits StopBits { get; }

        object Tag { get; set; }

        Response<byte[]> Request(byte[] bytes, int responseTimeout);

        void Reset(string portName = null, int? baudRate = null, Parity? parity = null, StopBits? stopBits = null, int? dataBits = null, bool? rtsEnable = null, Handshake? handshake = null);
    }
}