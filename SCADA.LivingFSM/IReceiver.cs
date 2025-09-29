namespace SCADA.LivingFSM
{
    public interface IReceiver
    {
        void RecvArgs(params object[] args);
    }
}