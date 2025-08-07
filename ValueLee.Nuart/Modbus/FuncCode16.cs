using System;

namespace Nuart.Modbus
{
    public static class FuncCode16
    {
        public static byte[] BuildRtuRequest(int slaveUnit, int startHoldingRegisterAddress, short[] values)
        {
            if (slaveUnit < 0 || slaveUnit > 255)
            {
                throw new ArgumentOutOfRangeException("从机地址必须介于[0,255]");
            }

            return null;
        }
    }
}