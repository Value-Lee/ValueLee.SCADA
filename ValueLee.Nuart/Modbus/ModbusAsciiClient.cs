namespace Nuart.Modbus
{
    public class ModbusAsciiClient : IModbusClient
    {
        public Response<bool[]> FC01(int slaveUnit, int startCoilAddress, int coilQuantity, int responseTimeout)
        {
            throw new System.NotImplementedException();
        }

        public Response<byte[]> FC03Byte(int slaveUnit, int startHoldingAddress, int quantity, int responseTimeout)
        {
            throw new System.NotImplementedException();
        }

        public Response<double[]> FC03Double(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder8 byteOrder, int responseTimeout)
        {
            throw new System.NotImplementedException();
        }

        public Response<float[]> FC03Float(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder4 byteOrder, int responseTimeout)
        {
            throw new System.NotImplementedException();
        }

        public Response<short[]> FC03Int16(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder2 byteOrder, int responseTimeout)
        {
            throw new System.NotImplementedException();
        }

        public Response<int[]> FC03Int32(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder4 byteOrder, int responseTimeout)
        {
            throw new System.NotImplementedException();
        }

        public Response<long[]> FC03Int64(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder8 byteOrder, int responseTimeout)
        {
            throw new System.NotImplementedException();
        }

        public Response<ushort[]> FC03UInt16(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder2 byteOrder, int responseTimeout)
        {
            throw new System.NotImplementedException();
        }

        public Response<uint[]> FC03UInt32(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder4 byteOrder, int responseTimeout)
        {
            throw new System.NotImplementedException();
        }

        public Response<ulong[]> FC03UInt64(int slaveUnit, int startHoldingAddress, int quantity, ByteOrder8 byteOrder, int responseTimeout)
        {
            throw new System.NotImplementedException();
        }
    }
}