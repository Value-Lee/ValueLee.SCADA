using System;

namespace SCADA.TimerFSM.Interfaces
{
    public interface IFsmMessenger
    {
        (bool isSuccess, Enum currentState) PostMsg(Enum msgCmd, params object[] args);
        void ClearMsgQueue();
    }
}