namespace SCADA.TimerFSM.Interfaces
{
    public interface IStateMachine : IFsmController, IFsmTransitionTable
    {
        public string Name { get;}
    }
}