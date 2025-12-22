using System;

namespace SCADA.TimerFSM
{
    public class ExceptionThrownEventArgs : EventArgs
    {
        public ExceptionThrownEventArgs(Exception exception, Enum currState, Enum nextState, Enum msgCmd, object[] msgArgs)
        {
            Exception = exception;
            CurrState = currState;
            NextState = nextState;
            MsgCmd = msgCmd;
            MsgArgs = msgArgs;
        }

        public Exception Exception { get; internal set; }
        public Enum CurrState { get; internal set; }
        public Enum NextState { get; internal set; }
        public Enum MsgCmd { get; internal set; }
        public object[] MsgArgs { get; internal set; }
    }

    public class StateTransitingEventArgs : EventArgs
    {
        public StateTransitingEventArgs(Enum currState, Enum nextState, Enum msgCmd, object[] msgArgs)
        {
            CurrState = currState;
            NextState = nextState;
            MsgCmd = msgCmd;
            MsgArgs = msgArgs;
        }

        public Enum CurrState { get; internal set; }
        public Enum NextState { get; internal set; }
        public Enum MsgCmd { get; internal set; }
        public object[] MsgArgs { get; internal set; }
    }

    public class StateTransitedEventArgs : EventArgs
    {
        public StateTransitedEventArgs(Enum currState, Enum previousState, Enum msgCmd, object[] msgArgs)
        {
            CurrState = currState;
            PreviousState = previousState;
            MsgCmd = msgCmd;
            MsgArgs = msgArgs;
        }

        public Enum CurrState { get; internal set; }
        public Enum PreviousState { get; internal set; }
        public Enum MsgCmd { get; internal set; }
        public object[] MsgArgs { get; internal set; }
    }

}