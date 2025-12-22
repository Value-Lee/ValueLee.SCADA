namespace SCADA.PLCFramework
{
    internal class PlcByteSnapshot
    {
        public PlcByteSnapshot()
        {

        }

        public long Timestamp { get; set; }
        public byte[] Values { get; set; }
    }
}
