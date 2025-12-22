using SCADA.Common;
using SCADA.TimerFSM.Enums;
using SCADA.TimerFSM.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SCADA.TimerFSM
{
    public abstract class RoutineBase : ITaktOptimizer
    {
        private readonly List<Enum> _historySteps;
        private readonly CountdownTimer _routineCountdownTimer;
        private readonly IFsmMessenger _stateMachine;
        private readonly CountdownTimer _stepCountdownTimer;
        private Enum _currStepId;
        private bool _executionExecuted;
        private bool _hasStarted = false;
        private bool _idle = true;

        //loop control
        private int _loopCounter = 0;

        private Enum _loopStepStartId;
        private int _loopTotalCountSetting = 0;
        private StepResult _token;

        public RoutineBase(IFsmController stateMachine)
        {
            _historySteps = new List<Enum>();
            _stateMachine = stateMachine;
            _stepCountdownTimer = new CountdownTimer(Timeout.InfiniteTimeSpan);
            _routineCountdownTimer = new CountdownTimer(Timeout.InfiniteTimeSpan);
            _token = StepResult.None;
            StepContext = new StepContext();
        }

        public Enum CurrentStepId => _currStepId;
        public StepResult RoutineStatus => _token;
        public StepContext StepContext { get; }

        public void Delay(Enum id, TimeSpan timeout)
        {
            if (PreprocessStep(id))
            {
                if (_executionExecuted == false)
                {
                    _stepCountdownTimer.Start(timeout);
                    _executionExecuted = true;
                }
                if (_stepCountdownTimer.IsTimeout)
                {
                    NextStep();
                }
                _token = StepResult.Proceed;
            }
        }

        public void EndLoop(Enum id)
        {
            if (_historySteps.Contains(id) == false)
            {
                ++_loopCounter;
                if (_loopCounter >= _loopTotalCountSetting) //Loop 结束
                {
                    _loopCounter = 0;
                    _loopTotalCountSetting = 0; // Loop 结束时，当前loop和loop总数都清零
                    NextStep();
                    _token = StepResult.Proceed;
                }
                else
                {
                    //继续下一LOOP
                    Rollback2Step(_loopStepStartId);
                    _token = StepResult.Proceed;
                }
            }
        }

        #region Implementation

        // 中途取消
        public bool Abort(params object[] args)
        {
            Abort();
            return true;
        }

        // 中途失败
        public bool Error(params object[] args)
        {
            Error();
            return true;
        }

        public bool Start(params object[] args)
        {
            Reset();
            StepContext.MsgArgs = args;
            bool ret;
            try
            {
                ret = Start();
            }
            catch (Exception ex)
            {
                StepContext.Exception = ex;
                return false;
            }
            return ret;
        }

        // 瞬时操作
        protected abstract void Abort();
        // 瞬时操作
        protected abstract void Error();

        // 瞬时操作
        protected abstract bool Start();

        protected abstract void Steps();

        #endregion Implementation

        #region Execute

        public void Execute(Enum id, StepAction execution)
        {
            ExecuteThenDelay(id, execution, TimeSpan.Zero);
        }

        #endregion Execute

        #region ExecuteThenCheck

        public void ExecuteThenCheck(Enum id, StepAction execution, StepCheck checker)
        {
            ExecuteThenCheck(id, execution, checker, TimeSpan.Zero);
        }

        public void ExecuteThenCheck(Enum id, StepAction execution, StepCheck checker, TimeSpan timeout)
        {
            if (PreprocessStep(id))
            {
                if (_executionExecuted == false)
                {
                    try
                    {
                        execution.Invoke(StepContext);
                    }
                    catch (Exception ex)
                    {
                        StepContext.Exception = ex;
                        _token = StepResult.Error;
                        return;
                    }
                    _executionExecuted = true;
                    if (timeout != TimeSpan.Zero)
                        _stepCountdownTimer.Start(timeout);
                }
            }

            bool checkResult;
            try
            {
                checkResult = checker.Invoke(StepContext);
            }
            catch (Exception ex)
            {
                StepContext.Exception = ex;
                _token = StepResult.Error;
                return;
            }

            if (checkResult)
            {
                NextStep();
            }
            if (timeout != TimeSpan.Zero)
                if (_stepCountdownTimer.IsTimeout)
                {
                    _token = StepResult.Error;
                    return;
                }
            _token = StepResult.Proceed;
        }

        #endregion ExecuteThenCheck

        #region ExecuteRoutine

        public void Execute(Enum id, RoutineBase routine, params object[] routineArgs)
        {
            if (PreprocessStep(id))
            {
                if (_executionExecuted == false)
                {
                    try
                    {
                        routine.Start(routineArgs);
                    }
                    catch (Exception ex)
                    {
                        StepContext.Exception = ex;
                        _token = StepResult.Error;
                        return;
                    }
                    _executionExecuted = true;
                }
                if (routine.Steps(null))
                {
                    NextStep();
                    _token = StepResult.Proceed;
                }
                else
                {
                    _token = StepResult.Proceed;
                }
            }
        }

        #endregion ExecuteRoutine

        #region ExecuteThenDelay

        public void ExecuteThenDelay(Enum id, StepAction execution, TimeSpan timeout)
        {
            if (PreprocessStep(id))
            {
                if (_executionExecuted == false)
                {
                    try
                    {
                        execution.Invoke(StepContext);
                    }
                    catch (Exception ex)
                    {
                        StepContext.Exception = ex;
                        _token = StepResult.Error;
                        return;
                    }
                    _executionExecuted = true;
                    if (timeout != TimeSpan.Zero)
                    {
                        _stepCountdownTimer.Start(timeout);
                    }
                }
                if (timeout != TimeSpan.Zero)
                {
                    if (_stepCountdownTimer.IsTimeout)
                    {
                        NextStep();
                    }
                }
                else
                {
                    NextStep();
                }

                _token = StepResult.Proceed;
            }
        }

        #endregion ExecuteThenDelay

        ITaktOptimizer ITaktOptimizer.FineTuneCheckInterval(TimeSpan timeSpan)
        {
            return (ITaktOptimizer)this;
        }

        public void Loop(Enum id, int loopCount)
        {
            if (_historySteps.Contains(id) == false)
            {
                _loopStepStartId = _currStepId;
                _loopTotalCountSetting = loopCount;
                NextStep();
                _token = StepResult.Proceed;
            }
        }

        public void Rollback2Step(Enum id)
        {
            int index = _historySteps.FindIndex(item => item.IsSame(id));
            if (_historySteps.Count > index + 1)
            {
                _historySteps.RemoveRange(index + 1, _historySteps.Count - index - 1);
            }
            _currStepId = _historySteps[index];
            _executionExecuted = false;
        }

        public bool Steps(params object[] args2)
        {
            // Routine出现Error后就不应该再执行任一Step了。
            // 比如已经Post Error Message了，但是Error Message对应的Action返回false，状态仍旧
            // 还是原来的状态，FsmMsgCmd.Timer还会驱动MonitorSteps，这要预防！
            // Error 和 Completed状态下都不应该继续执行Step
            if (_token != StepResult.Proceed && _token != StepResult.None)
                return false;

            if (_hasStarted == false)
            {
                RoutineStart();
                _hasStarted = true;
            }

            Steps();

            if (_token == StepResult.Error)
            {
                ((IFsmMessenger)_stateMachine).PostMsg(FsmMsgCmd.Error, null);
                return false;
            }

            if (_historySteps.Count > 0 && _token == StepResult.Proceed)
            {
                RoutineComplete();
            }

            return _token == StepResult.Completed;
        }

        private void NextStep()
        {
            _historySteps.Add(_currStepId);
            _executionExecuted = false;
            _idle = true;
            // 改成立刻执行下一步
        }

        private bool PreprocessStep(Enum id)
        {
            // 已经执行过该Step
            if (_historySteps.Contains(id) == false)
                return false;
            // 发现的第一个未执行的Step作为当前Step
            if (_idle == true)
            {
                _currStepId = id;
                _idle = false;
            }
            // 其他未执行的Step暂时先跳过
            if (_currStepId.IsSame(id) == false)
                return false;
            return true;
        }

        private void Reset()
        {
            _token = StepResult.Proceed;
            _currStepId = RoutineStepId.None;
            _historySteps.Clear();
            _loopStepStartId = RoutineStepId.None;
            _loopCounter = 0;
            _loopTotalCountSetting = 0;
            _executionExecuted = false;
            _hasStarted = false;
            _idle = true;
            StepContext.Exception = null;
            StepContext.Error = null;
            StepContext.ClearStepData();
            StepContext.MsgArgs = null;
            StepContext.SharedVariable = null;
        }

        private void RoutineComplete()
        {
            _token = StepResult.Completed;
            ((IFsmMessenger)(this)).PostMsg(FsmMsgCmd.Complete, null);
        }

        private void RoutineStart()
        {
            _token = StepResult.Proceed;
        }
    }
}