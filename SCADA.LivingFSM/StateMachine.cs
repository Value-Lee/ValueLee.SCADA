using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SCADA.LivingFSM
{
    public class StateMachine : IStateMachine
    {
        private CancellationTokenSource _cancellationTokenSource;

        private Enum _currState;
        private LinkedList<Enum> _historyStates = new LinkedList<Enum>();
        private int _interval = 75;
        private Queue<LongTimeActionBase> _longTimeActions = new Queue<LongTimeActionBase>();

        //private BlockingCollection<(Enum msgCmmd, object[] args)> _msgQueue;
        private Channel<(Enum msgCmmd, object[] args)> _msgQueue;

        private TaskCompletionSource<bool> _pauseEvent;
        private Dictionary<string, List<(Enum msgCmd, Func<object[], bool> action, Enum nextState)>> _transitionTable;

        public event EventHandler<StateTransitedEventArgs> MsgNotMatchAnyState;

        public event EventHandler<StateTransitedEventArgs> StateEntered;

        public event EventHandler<StateTransitedEventArgs> StateExited;

        public object Tag { get; set; }

        public void AdjustInterval(int intervalMS)
        {
            if (intervalMS < -1)
            {
                throw new ArgumentOutOfRangeException("intervalMS");
            }
            _interval = intervalMS;
        }

        #region Constructors

        public StateMachine() : this(FsmState.Pangu)
        {
        }

        public StateMachine(Enum initialState) : this(initialState, 75)
        {
        }

        public StateMachine(Enum initialState, int interval)
        {
            _transitionTable = new Dictionary<string, List<(Enum msg, Func<object[], bool> action, Enum nextState)>>();
            _msgQueue = Channel.CreateUnbounded<(Enum msgCmd, object[] msgArgs)>();
            _pauseEvent = new TaskCompletionSource<bool>(false, TaskCreationOptions.RunContinuationsAsynchronously);
            //new Thread(Loop).Start();
            _currState = initialState;
            _interval = interval;
            Task.Run(Loop);
        }

        #endregion Constructors

        #region IStateTracker

        Enum IStateMachine.CurrState => _currState;
        int IStateMachine.HistoryStateCapacity => throw new NotImplementedException();
        Enum IStateMachine.PrevState => ((IStateMachine)this).GetState(1);

        Enum IStateMachine.GetState(int prevIndex)
        {
            return _historyStates.ElementAt(prevIndex);
        }

        #endregion IStateTracker

        bool IStateMachine.CanMatch(Enum msgCmd)
        {
            return ((IStateMachine)this).CanMatch(msgCmd, _currState);
        }

        bool IStateMachine.CanMatch(Enum msgCmd, Enum state)
        {
            var anyHashCode = FsmState.Any.GetHashStringCode();
            if (_transitionTable.ContainsKey(anyHashCode) && _transitionTable[anyHashCode].Any(item => item.msgCmd.IsSame(msgCmd)))
                return true;
            var stateHashCode = state.GetHashStringCode();
            if (_transitionTable.ContainsKey(stateHashCode) && _transitionTable[stateHashCode].Any(item => item.msgCmd.IsSame(msgCmd)))
                return true;
            return false;
        }

        bool IStateMachine.CanMatch(Enum msgCmd, Enum state, out Func<object[], bool> action, out Enum nextState)
        {
            action = default;
            nextState = default;

            var anyHashCode = FsmState.Any.GetHashStringCode();
            if (_transitionTable.ContainsKey(anyHashCode))
            {
                var index = _transitionTable[anyHashCode].FindIndex(item => item.msgCmd.IsSame(msgCmd));
                if (index > -1)
                {
                    action = _transitionTable[anyHashCode][index].action;
                    nextState = _transitionTable[anyHashCode][index].nextState;
                    return true;
                }
            }
            var stateHashCode = state.GetHashStringCode();
            if (_transitionTable.ContainsKey(stateHashCode))
            {
                var index = _transitionTable[stateHashCode].FindIndex(item => item.msgCmd.IsSame(msgCmd));
                if (index > -1)
                {
                    action = _transitionTable[stateHashCode][index].action;
                    nextState = _transitionTable[stateHashCode][index].nextState;
                    return true;
                }
            }
            return false;
        }

        bool IStateMachine.CanMatch(Enum msgCmd, out Func<object[], bool> action, out Enum nextState) => ((IStateMachine)this).CanMatch(msgCmd, _currState, out action, out nextState);

        public void Pause()
        {
            var old = Interlocked.CompareExchange(ref _pauseEvent, new TaskCompletionSource<bool>(false), null);
            if (old == null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public void PostMsg(Enum msgCmd, params object[] args)
        {
            _msgQueue.Writer.TryWrite((msgCmd, args));
        }

        public void Register(Enum currState, Enum nextState, Enum msgCmd, Func<object[], bool> action)
        {
            var stateHashCode = currState.GetHashStringCode();
            if (!_transitionTable.ContainsKey(stateHashCode))
            {
                _transitionTable[stateHashCode] = new List<(Enum msg, Func<object[], bool> action, Enum nextState)>();
            }
            _transitionTable[stateHashCode].Add((msgCmd, action, nextState));
        }

        public void Register(Enum currState, Enum nextState, Enum msgCmd)
        {
            Register(currState, nextState, msgCmd, (args) => true);
        }

        public void Register(Enum currState, Enum nextState, Enum msgCmd, Enum relayState, LongTimeActionBase longTimeAction)
        {
            Register(currState, relayState, msgCmd, (args) =>
            {
                ((IReceiver)longTimeAction).RecvArgs(args);
                var can = longTimeAction.CanAction();
                if (!can) { longTimeAction.OnCannotAction(); }
                return can;
            });

            Register(relayState, currState, FsmMsgCmd.Abort, (args) =>
            {
                longTimeAction.Abort(longTimeAction.StepResultToken.Parameter);
                return true;
            });

            Register(relayState, nextState, FsmMsgCmd.Timer, (args) =>
            {
                var result = longTimeAction.Steps();

                // Long Time Action Finished.
                switch (result.Result)
                {
                    case Result.Finished:
                        longTimeAction.Finished(args);
                        return true;

                    case Result.Forward:
                        longTimeAction.Forward(args);
                        ; // Do Nothing
                        return false;

                    case Result.Failed:
                        return longTimeAction.Failed(longTimeAction.StepResultToken.Parameter);
                }
                throw new ArgumentException();
            });
        }

        public void Resume()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _pauseEvent?.SetResult(true);
        }

        private async void Loop()
        {
            while (true)
            {
                var pauseEvent = Interlocked.CompareExchange(ref _pauseEvent, null, null);
                if (pauseEvent != null)
                {
                    await pauseEvent.Task.ConfigureAwait(false);// 暂停机制
                }

                (Enum cmd, object[] args) msg;

                var delayTask = Task.Delay(_interval);
                var readTask = _msgQueue.Reader.ReadAsync().AsTask();
                var completedTask = await Task.WhenAny(delayTask, readTask).ConfigureAwait(false);
                if (completedTask == readTask)
                {
                    msg = await readTask.ConfigureAwait(false);
                }
                else
                {
                    msg = (FsmMsgCmd.Timer, null);
                }

                // 不管当前是什么状态，全部忽略之，强制执行switch default 对应的逻辑，并强制切换到指定状态。换句话说，任何状态都要处理此消息，执行action后，跳转到指定的状态。
                var match = ((IStateMachine)this).CanMatch(msg.cmd, out Func<object[], bool> action, out Enum nextState);
                // 当前状态不接受此消息
                if (!match)
                {
                    if (!msg.cmd.IsSame(FsmMsgCmd.Timer))
                    {
                        MsgNotMatchAnyState?.Invoke(this, new StateTransitedEventArgs(msg.cmd, _currState));
                    }
                    continue;
                }

                // Execute Action
                var result = action.Invoke(msg.args);

                // 成功切换状态
                if (result)
                {
                    StateExited?.Invoke(this, new StateTransitedEventArgs(msg.cmd, _currState));
                    _currState = nextState;
                    StateEntered?.Invoke(this, new StateTransitedEventArgs(msg.cmd, _currState));
                }
            }
        }
    }
}