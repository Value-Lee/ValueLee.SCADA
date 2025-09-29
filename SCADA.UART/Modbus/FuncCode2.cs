using SCADA.Common;
using System;
using System.IO;

namespace SCADA.UART.Modbus
{
    public static class FuncCode2
    {
        public static byte[] BuildRtuRequest(int slaveUnit, int startDiscreteInputAddress, int discreteInputQuantity)
        {
            ArgumentChecker.CheckRegisterAddr01(slaveUnit);
            ArgumentChecker.CheckRegisterAddr01(startDiscreteInputAddress);
            ArgumentChecker.CheckRegisterQuantity01(discreteInputQuantity);

            var bytes = new byte[8];
            bytes[0] = (byte)slaveUnit;
            bytes[1] = 1;
            byte high, low;
            var binaryRepresentation = BitConverter.GetBytes((ushort)startDiscreteInputAddress);
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

            binaryRepresentation = BitConverter.GetBytes((ushort)discreteInputQuantity);
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

        public static bool ResolveRtuResponse(byte[] response, out int slaveUnit, out bool[] values, out byte exceptionCode)
        {
            slaveUnit = 0;
            values = null;
            exceptionCode = 0;

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

            return true;
        }
    }
}