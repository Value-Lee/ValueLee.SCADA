using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA.Common
{
    public sealed class PeriodicTimer : IDisposable
    {
        private readonly object _locker;
        private CancellationTokenSource _cts;
        private bool _disposed;
        private volatile int _periodMS;
        private Timer _timer;

        public PeriodicTimer() : this(100)
        {
        }

        public PeriodicTimer(int periodMS)
        {
            if (periodMS < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(periodMS), "Period must be non-negative.");
            }
            _locker = new object();
            _periodMS = periodMS;
        }

        public event Action<CancellationToken> Callback;

        public event Action<Exception> CallbackExceptionOccured;

        public bool IsCallbackEmpty => Callback == null;

        public bool ContinueOtherTasksWhenExceptionOccured { get; set; } = true;

        // 这个新的周期值只会在下一次回调执行完毕后才生效。
        public void ChangePeriod(int newPeriodMS)
        {
            if (newPeriodMS < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newPeriodMS), "Period must be non-negative.");
            }
            _periodMS = newPeriodMS;
        }

        public void Dispose()
        {
            lock (_locker)
            {
                if (!_disposed)
                {
                    _timer?.Dispose();
                    _timer = null;
                    Callback = null;
                    CallbackExceptionOccured = null;
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _cts = null;
                    GC.SuppressFinalize(this);
                    _disposed = true;
                }
            }
        }

        public void Start()
        {
            lock (_locker)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(PeriodicTimer), "Cannot start a disposed PeriodicTimer instance.");
                }
                if (_timer == null)
                {
                    _cts = new CancellationTokenSource();
                    _timer = new Timer((state) =>
                    {
                        var callback = Callback;

                        if (callback == null)
                        {
                            return; // No subscribers, nothing to do
                        }
                        foreach (var del in callback?.GetInvocationList())
                        {
                            var cts = _cts;
                            if (cts == null || cts.Token.IsCancellationRequested)
                            {
                                break;
                            }
                            try
                            {
                                //del.DynamicInvoke(cts.Token); // low performance
                                ((Action<CancellationToken>)del)?.Invoke(cts.Token);
                            }
                            catch (Exception ex)
                            {
                                CallbackExceptionOccured?.Invoke(ex);

                                if (!ContinueOtherTasksWhenExceptionOccured)
                                    break; // Stop invoking further subscribers on exception
                            }
                        }
                        try
                        {
                            _timer?.Change(_periodMS, Timeout.Infinite);
                        }
                        catch (ObjectDisposedException)
                        {
                            ; // empty
                        }
                    }, null, Timeout.Infinite, Timeout.Infinite);

                    _timer?.Change(0, Timeout.Infinite);
                }
            }
        }

        public bool Stop(int timeoutMS = 0)
        {
            if (timeoutMS < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutMS), "Timeout must be non-negative.");
            }

            lock (_locker)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(PeriodicTimer), "Cannot stop a disposed PeriodicTimer instance.");
                }
                _cts?.Cancel();

                if (_timer != null)
                {
                    bool ret = WaitForDispose(_timer, TimeSpan.FromMilliseconds(timeoutMS));
                    // delay dispose and nulling of _cts so that callbacks can still check the token
                    _cts?.Dispose();
                    _cts = null;
                    return ret;
                }
                return true;
            }

            bool WaitForDispose(Timer timer, TimeSpan timeout)
            {
                if (timer != null)
                {
                    timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan); // Stop generating new callbacks
                    if (timeout != TimeSpan.Zero)
                    {
                        ManualResetEvent waitHandle = new ManualResetEvent(false);
                        if (timer.Dispose(waitHandle) && !waitHandle.WaitOne((int)timeout.TotalMilliseconds))
                        {
                            return false;
                        }
                        waitHandle.Close();
                    }
                    else
                    {
                        timer.Dispose();
                    }
                }
                timer = null;
                return true;
            }
        }
    }
}