using System;

namespace SCADA.TimerFSM.Interfaces
{
    public interface IFsmTransitionTable
    {
        bool CanMatch(Enum msgCmd, Enum state);

        bool CanMatch(Enum msgCmd, Enum state, out Func<object[], bool> action, out Enum nextState);

        void Register(Enum currState, Enum nextState, Enum msgCmd, Func<object[], bool> action);

        void Register(Enum currState, Enum nextState, Enum msgCmd);

        void Register(Enum currState, RoutineBase routine, Enum msgCmd, Enum relayState, Enum nextState, Enum abortState);
    }
}