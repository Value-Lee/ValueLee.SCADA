using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuart.Modbus
{
    public static class ArgumentChecker
    {
        public static void CheckRegisterAddr01(int address)
        {
            CheckRegisterAddressRange(0x0000, 0xFFFF, address);
        }

        public static void CheckRegisterAddr02(int address)
        {
            CheckRegisterAddressRange(0x0000, 0xFFFF, address);
        }

        public static void CheckRegisterAddr03(int address)
        {
            CheckRegisterAddressRange(0x0000, 0xFFFF, address);
        }

        public static void CheckRegisterAddr04(int address)
        {
            CheckRegisterAddressRange(0x0000, 0xFFFF, address);
        }

        public static void CheckRegisterAddr05(int address)
        {
            CheckRegisterAddressRange(0x0000, 0xFFFF, address);
        }

        public static void CheckRegisterAddr06(int address)
        {
            CheckRegisterAddressRange(0x0000, 0xFFFF, address);
        }

        public static void CheckRegisterAddr15(int address)
        {
            CheckRegisterAddressRange(0x0000, 0xFFFF, address);
        }

        public static void CheckRegisterAddr16(int address)
        {
            CheckRegisterAddressRange(0x0000, 0xFFFF, address);
        }

        public static void CheckRegisterAddr23(int address)
        {
            CheckRegisterAddressRange(0x0000, 0xFFFF, address);
        }

        public static void CheckRegisterQuantity01(int address)
        {
            CheckRegisterQuantityRange(0x0001, 0x07d0, address);
        }

        public static void CheckRegisterQuantity02(int address)
        {
            CheckRegisterQuantityRange(0x0001, 0x07d0, address);
        }

        public static void CheckRegisterQuantity03(int address)
        {
            CheckRegisterQuantityRange(0x0001, 0x007d, address);
        }

        public static void CheckRegisterQuantity04(int address)
        {
            CheckRegisterQuantityRange(0x0001, 0x007d, address);
        }

        public static void CheckRegisterQuantity05(int address)
        {
            CheckRegisterQuantityRange(0x0001, 0x0001, address);
        }

        public static void CheckRegisterQuantity06(int address)
        {
            CheckRegisterQuantityRange(0x0001, 0x0001, address);
        }

        public static void CheckRegisterQuantity15(int address)
        {
            CheckRegisterQuantityRange(0x0001, 0x07b0, address);
        }

        public static void CheckRegisterQuantity16(int address)
        {
            CheckRegisterQuantityRange(0x0001, 0x007b, address);
        }

        public static void CheckRegisterQuantity23(int address)
        {
            CheckRegisterQuantityRange(0x0001, 0x07d0, address);
        }

        public static void CheckSlaveUnit01(int address)
        {
            CheckSlaveUnitRange(1, 255, address);
        }

        public static void CheckSlaveUnit02(int address)
        {
            CheckSlaveUnitRange(1, 255, address);
        }

        public static void CheckSlaveUnit03(int address)
        {
            CheckSlaveUnitRange(1, 255, address);
        }

        public static void CheckSlaveUnit04(int address)
        {
            CheckSlaveUnitRange(1, 255, address);
        }

        public static void CheckSlaveUnit05(int address)
        {
            CheckSlaveUnitRange(1, 255, address);
        }

        public static void CheckSlaveUnit06(int address)
        {
            CheckSlaveUnitRange(1, 255, address);
        }

        public static void CheckSlaveUnit15(int address)
        {
            CheckSlaveUnitRange(1, 255, address);
        }

        public static void CheckSlaveUnit16(int address)
        {
            CheckSlaveUnitRange(1, 255, address);
        }

        public static void CheckSlaveUnit23(int address)
        {
            CheckSlaveUnitRange(1, 255, address);
        }

        private static void CheckRegisterAddressRange(int min, int max, int address)
        {
            if (address < min || address > max)
            {
                var ex = new ArgumentOutOfRangeException(nameof(address), $"valid range: [{min},{max}]");
                ex.Data.Add(nameof(address), address);
                throw ex;
            }
        }

        private static void CheckRegisterQuantityRange(int min, int max, int quantity)
        {
            if (quantity < min || quantity > max)
            {
                var ex = new ArgumentOutOfRangeException(nameof(quantity), $"valid range: [{min},{max}]");
                ex.Data.Add(nameof(quantity), quantity);
                throw ex;
            }
        }

        private static void CheckSlaveUnitRange(int min, int max, int address)
        {
            if (address < min || address > max)
            {
                var ex = new ArgumentOutOfRangeException(nameof(address), $"valid range: [{min},{max}]");
                ex.Data.Add(nameof(address), address);
                throw ex;
            }
        }
    }
}