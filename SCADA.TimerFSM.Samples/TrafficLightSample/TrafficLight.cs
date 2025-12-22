using SCADA.Common;
using SCADA.TimerFSM.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCADA.TimerFSM.Samples.TrafficLightSample
{
    internal class TrafficLight
    {
        public enum TrafficLightState
        {
            Red,
            Green,
            Yellow
        }
        public enum TrafficLightCommand
        {
            Red,
            Green,
            Yellow
        }

        public TrafficLight()
        {
            _stateMachine = new StateMachine("stop-light");
            _countdownTimer = new CountdownTimer(1000 * 30);
            RegisterStateTable();
        }

        private StateMachine _stateMachine;


        public void PostMsg(Enum msg)
        {
            _stateMachine.PostMsg(msg);
        }

        public void RegisterStateTable()
        {
            _stateMachine.Register(FsmState.None, TrafficLightState.Green, TrafficLightCommand.Green);
            _stateMachine.Register(TrafficLightState.Green, TrafficLightState.Yellow, TrafficLightCommand.Yellow);
            _stateMachine.Register(TrafficLightState.Yellow, TrafficLightState.Red, TrafficLightCommand.Red);
            _stateMachine.Register(TrafficLightState.Red, TrafficLightState.Green, TrafficLightCommand.Green);
            _stateMachine.Register(TrafficLightState.Green, FsmState.None, TrafficLightCommand.Red);

            /***/
            _stateMachine.SetMonitor(TrafficLightState.Green, () =>
            {
                if (_countdownTimer.IsTimeout)
                {
                    _stateMachine.PostMsg(TrafficLightCommand.Yellow);
                }
            });

            _stateMachine.SetMonitor(TrafficLightState.Yellow, () =>
            {
                if (_countdownTimer.IsTimeout)
                {
                    _stateMachine.PostMsg(TrafficLightCommand.Red);
                }
            });

            _stateMachine.SetMonitor(TrafficLightState.Red, () =>
            {
                if (_countdownTimer.IsTimeout)
                {
                    _stateMachine.PostMsg(TrafficLightCommand.Green);
                }
            });

            _stateMachine.StateEntered += StateMachine_StateEntered;
        }

        private readonly CountdownTimer _countdownTimer;

        private void StateMachine_StateEntered(object? sender, StateTransitedEventArgs e)
        {
            if (e.CurrState.IsSame(TrafficLightState.Green))
            {
                _countdownTimer.Start(1000 * 10);
            }
            else if (e.CurrState.IsSame(TrafficLightState.Yellow))
            {
                _countdownTimer.Start(1000 * 3);
            }
            else if (e.CurrState.IsSame(TrafficLightState.Red))
            {
                _countdownTimer.Start(1000 * 15);
            }
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Console.WriteLine($"{now}:{e.PreviousState}->{e.CurrState}");
        }
    }
}
