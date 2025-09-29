using Nuart;
using SCADA.Common;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SCADA.UART.Modbus
{
    public static class FuncCode1
    {
        public static string BuildAsciiRequest(int slaveUnit, int startCoilAddress, int coilCount)
        {
            var sb = new StringBuilder();
            sb.Append(":");
            sb.Append(BuildRtuRequest(slaveUnit, startCoilAddress, coilCount).Select(item => item.ToString("X2")));
            sb.Append("\r\n");
            return sb.ToString();
        }

        public static byte[] BuildRtuRequest(int slaveUnit, int startCoilAddress, int coilQuantity)
        {
            ArgumentChecker.CheckSlaveUnit01(slaveUnit);
            ArgumentChecker.CheckRegisterAddr01(startCoilAddress);
            ArgumentChecker.CheckRegisterQuantity01(coilQuantity);

            var bytes = new byte[8];
            bytes[0] = (byte)slaveUnit;
            bytes[1] = 1;
            byte high, low;
            var binaryRepresentation = BitConverter.GetBytes((ushort)startCoilAddress);
            if (BitConverter.IsLittleEndian)
            {
                low = binaryRepresentation[0];
                high = binaryRepresentation[1];
            }
            else
            {
                low = binaryRepresentation[1];
                high = binaryRepresentation[0];
            }
            bytes[2] = high;
            bytes[3] = low;

            binaryRepresentation = BitConverter.GetBytes((ushort)coilQuantity);
            if (BitConverter.IsLittleEndian)
            {
                low = binaryRepresentation[0];
                high = binaryRepresentation[1];
            }
            else
            {
                low = binaryRepresentation[1];
                high = binaryRepresentation[0];
            }
            bytes[4] = high;
            bytes[5] = low;

            (high, low) = DataVerifier.Crc16Modbus(bytes, 0, 6);
            bytes[6] = low;
            bytes[7] = high;
            return bytes;
        }

        public static byte[] BuildTcpRequest()
        {
            throw new NotImplementedException();
        }

        public static bool[] ResolveAsciiResponse(string response)
        {
            throw new NotImplementedException();
        }

        public static bool ResolveRtuResponse(byte[] response, out int slaveUnit, out bool[] values, out byte exceptionCode)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            // CRC校验
            var length = response.Length;
            var high = response[length - 1];
            var low = response[length - 2];
            var (high2, low2) = DataVerifier.Crc16Modbus(response, 0, length - 2);
            if (high != high2 || low != low2)
            {
                throw new InvalidDataException("CRC16 check failed.");
            }
            // 解析Slave地址
            slaveUnit = response[0];
            // 解析功能码
            int function = response[1];
            if (function == 0x81)
            {
                exceptionCode = response[2];
                values = null;
                return false;
            }
            if (function != 0x01)
            {
                throw new MissingMatchException($"Function code in Response is {function}, and it isn't equal to 0x01.");
            }
            // 解析数据
            var bytesCount = response[2];
            values = new bool[bytesCount * 8];
            for (int i = 3; i < bytesCount + 3; i++)
            {
                Array.Copy(Utility.GetBitValues(response[i]), 0, values, (i - 3) * 8, 8);
            }

            exceptionCode = 0x00;
            return true;
        }

        public static bool[] ResolveTcpResponse()
        {
            throw new NotImplementedException();
        }
    }
}