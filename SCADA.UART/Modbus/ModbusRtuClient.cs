//using System;
//using System.CodeDom;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;
//using System.IO.Ports;
//using System.Linq;
//using System.Text;
//using Nuart.RequestReplyModel;

//namespace Nuart.Modbus
//{
//    public class ModbusRtuClient : IModbusClient, ISerialPort
//    {
//        private SerialPort<ModbusRtuFilter> serialInterface;

//        public ModbusRtuClient(string portName) : this(portName, 9600)
//        {
//        }

//        public ModbusRtuClient(string portName, int baudRate) : this(portName, baudRate, Parity.None, StopBits.One)
//        {
//        }

//        public ModbusRtuClient(string portName, int baudRate, Parity parity, StopBits stopBits, int dataBits, bool rtsEnable, Handshake handshake)
//        {
//            serialInterface = new SerialPort<ModbusRtuFilter>(portName, baudRate, parity, stopBits, dataBits, rtsEnable, handshake);
//        }

//        public ModbusRtuClient(string portName, int baudRate, Parity parity, StopBits stopBits, int dataBits) : this(portName, baudRate, parity, stopBits, dataBits, false, Handshake.None)
//        {
//        }

//        public ModbusRtuClient(string portName, int baudRate, Parity parity, StopBits stopBits) : this(portName, baudRate, parity, stopBits, 8)
//        {
//        }

//        event Action<SerialEventArgs<byte[]>> ISerialPort.CompletedFrameReceived
//        {
//            add
//            {
//                serialInterface.CompletedFrameReceived += value;
//            }

//            remove
//            {
//                serialInterface.CompletedFrameReceived -= value;
//            }
//        }

//        event Action<SerialEventArgs<byte[]>> ISerialPort.DataRead
//        {
//            add
//            {
//                serialInterface.DataRead += value;
//            }

//            remove
//            {
//                serialInterface.DataRead -= value;
//            }
//        }

//        event Action<SerialEventArgs<byte[]>> ISerialPort.DataSent
//        {
//            add
//            {
//                serialInterface.DataSent += value;
//            }

//            remove
//            {
//                serialInterface.DataSent -= value;
//            }
//        }

//        event Action<SerialEventArgs<Exception>> ISerialPort.TimedDataReadingJobThrowException
//        {
//            add
//            {
//                serialInterface.TimedDataReadingJobThrowException += value;
//            }

//            remove
//            {
//                serialInterface.TimedDataReadingJobThrowException -= value;
//            }
//        }

//        public int BaudRate => serialInterface.BaudRate;
//        public int DataBits => serialInterface.DataBits;
//        public Handshake Handshake => serialInterface.Handshake;
//        public int LastCompletedFrameResolvedTime => serialInterface.LastCompletedFrameResolvedTime;
//        public Parity Parity => serialInterface.Parity;
//        public string PortName => serialInterface.PortName;
//        int ISerialPort.RecvBuffLength => serialInterface.RecvBuffLength;
//        public bool RtsEnable => serialInterface.RtsEnable;
//        public StopBits StopBits => serialInterface.StopBits;
//        public object Tag { get => serialInterface.Tag; set => serialInterface.Tag = value; }

//        Response<byte[]> ISerialPort.Request(byte[] bytes, int waitResponseTimeout)
//        {
//            return serialInterface.Request(bytes, waitResponseTimeout);
//        }

//        public void Reset(string portName = null, int? baudRate = null, Parity? parity = null, StopBits? stopBits = null, int? dataBits = null, bool? rtsEnable = null, Handshake? handshake = null)
//        {
//            serialInterface.Reset(portName, baudRate, parity, stopBits, dataBits, rtsEnable, handshake);
//        }

//        #region Modbus

//        public Response<bool[]> FC01(int slaveUnit, int startAddress, int quantity, int responseTimeout)
//        {
//            try
//            {
//                var reqBytes = Modbus.FuncCode1.BuildRtuRequest(slaveUnit, startAddress, quantity);
//                var response = serialInterface.Request(reqBytes, responseTimeout);
//                if (response.IsSuccess)
//                {
//                    var lgth = response.Data.Length;
//                    (int high, int low) = DataVerifier.Crc16Modbus(response.Data, 0, lgth - 2);
//                    if (high != response.Data[lgth - 1] || low != response.Data[lgth - 2])
//                    {
//                        return new Response<bool[]>(null, "CRC16 check failed.");
//                    }

//                    bool normal = Modbus.FuncCode1.ResolveRtuResponse(response.Data, out int slaveUnit2, out bool[] values, out byte exceptionCode);
//                    if (normal == false)
//                    {
//                        return new Response<bool[]>(null, $"exception code: {exceptionCode}: exception description: {ModbusExceptionCodeTable.Instance.GetDescription(exceptionCode)}");
//                    }
//                    if (slaveUnit2 != slaveUnit)
//                        return new Response<bool[]>(null, $"Slave address in response is {slaveUnit2},and it is not same as the corresponding request's.");
//                    return new Response<bool[]>(values.Take(quantity).ToArray(), response.Exception, response.ErrorMsg);
//                }
//                else
//                {
//                    return new Response<bool[]>(null, response.Exception, response.ErrorMsg);
//                }
//            }
//            catch (Exception ex)
//            {
//                return new Response<bool[]>(null, ex);
//            }
//        }

//        public Response<byte[]> FC03Byte(int slaveUnit, int startHoldingAddress, int quantity, int responseTimeout)
//        {
//            throw new NotImplementedException();
//        }

//        public Response<double[]> FC03Double(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder8 byteOrder, int responseTimeout)
//        {
//            throw new NotImplementedException();
//        }

//        public Response<float[]> FC03Float(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder4 byteOrder, int responseTimeout)
//        {
//            throw new NotImplementedException();
//        }

//        public Response<short[]> FC03Int16(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder2 byteOrder, int responseTimeout)
//        {
//            throw new NotImplementedException();
//        }

//        public Response<int[]> FC03Int32(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder4 byteOrder, int responseTimeout)
//        {
//            throw new NotImplementedException();
//        }

//        public Response<long[]> FC03Int64(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder8 byteOrder, int responseTimeout)
//        {
//            throw new NotImplementedException();
//        }

//        public Response<ushort[]> FC03UInt16(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder2 byteOrder, int responseTimeout)
//        {
//            throw new NotImplementedException();
//        }

//        public Response<uint[]> FC03UInt32(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder4 byteOrder, int responseTimeout)
//        {
//            throw new NotImplementedException();
//        }

//        public Response<ulong[]> FC03UInt64(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder8 byteOrder, int responseTimeout)
//        {
//            throw new NotImplementedException();
//        }

//        #endregion Modbus
//    }
//}