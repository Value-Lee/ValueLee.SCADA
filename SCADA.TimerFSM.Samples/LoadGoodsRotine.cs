//using SCADA.TimerFSM.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading;

//namespace SCADA.TimerFSM
//{
//    public class Module1: IFsmController
//    {
//        enum FsmState
//        {
//            None,
//            Idle,
//            Running,
//            Paused,
//            Stopped
//        }

//        enum MsgCmd
//        {
//            Start,
//            Pause,
//            Resume,
//            Stop,
//        }

//        public void Go2Pos(double x)
//        {

//        }

//        public double GetPos()
//        {
//            return 0;
//        }

//        StateMachine _stateMachine;

//        public Enum CurrState => throw new NotImplementedException();

//        public Enum PrevState => throw new NotImplementedException();

//        public Module1()
//        {
//            _stateMachine = new StateMachine(FsmState.None);
//        }

//        public void RegisterTransitionTable()
//        {
//            _stateMachine.Register(FsmState.Idle, new LoadGoodsRotine(), MsgCmd.Start, FsmState.Running, FsmState.Idle,FsmState.Idle);

//        }

//        public void PostMsg(Enum msgCmd, params object[] args)
//        {
//            throw new NotImplementedException();
//        }

//        public void Resume()
//        {
//            throw new NotImplementedException();
//        }

//        public void Pause()
//        {
//            throw new NotImplementedException();
//        }
//    }


//    internal class LoadGoodsRotine : RoutineBase
//    {
//        public static Module1 _module1 = new Module1();

//        enum StepId
//        {
//            Go2PickPosition,
//            InPlace,
//            Delay1,
//            Pick,
//            Go2ScanPosition,
//            Delay2,
//            ScanCode,
//            Go2PlacePosition,
//            Delay3,
//            Place,
//            Back2Idle,
//        }

//        public LoadGoodsRotine() : base(_module1)
//        {

//        }

//        protected override bool Start()
//        {
//            var routineStartArgs = StepContext.MsgArgs;
//            // 时间必须极短
//            return true;
//        }

//        protected override void Abort()
//        {
//            throw new NotImplementedException();
//        }

      

//        protected override void Error()
//        {
//            var errorOnThisStep = CurrentStepId;
//            var errorMsg = StepContext.Error;
//            var ex = StepContext.Exception;
//            var routineStartArgs = StepContext.MsgArgs;
//            var p1 = StepContext.TryGetStepData("param1");

//            // 清理工作
//            // 此函数结束后，状态机会进入Error状态
//            // 后面考虑将函数的返回值设计成void

//        }

//        private int a;

//        protected override void Steps()
//        {
//            ExecuteThenCheck(StepId.Go2PickPosition,
//                () => { _module1.Go2Pos(1); return true; },
//                () => _module1.GetPos() == 1.0);
//            Delay(StepId.Delay1, TimeSpan.FromSeconds(1));
//            Execute(StepId.Pick, GetStepAction);
//            Execute(StepId.Go2ScanPosition, () => true);
//            Delay(StepId.Delay2, TimeSpan.FromSeconds(2));
//            Execute(StepId.ScanCode, () => true);
//            Execute(StepId.Go2PlacePosition, () => true);
//            Delay(StepId.Delay3, TimeSpan.FromSeconds(3));
//            ExecuteThenDelay(StepId.Place, () => true, TimeSpan.FromSeconds(4));
//            Execute(StepId.Back2Idle, () => true);
//        }

//        public void GetStepAction(StepContext context)
//        {
//            context.SharedVariable = null;
//        }
//    }
//}
