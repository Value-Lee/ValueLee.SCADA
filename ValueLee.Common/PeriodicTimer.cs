using System;
using System.Threading;
using System.Threading.Tasks;

namespace ValueLee.Common
{
    public sealed class PeriodicTimer : IDisposable
    {
        private readonly object _locker;
        private bool _disposed;
        private int _periodMS;
        private Timer _timer;

        public PeriodicTimer() : this(100)
        {
        }

        public PeriodicTimer(int periodMS)
        {
            if(periodMS <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(periodMS), "Period must be greater than zero.");
            }
            _locker = new object();
            _periodMS = periodMS;
        }

        public event Action<Exception> CallbackExceptionOccured;

        public event Action TimerCallback;

        internal PeriodicTimerCache TimerCache { get; set; }

        public void Dispose()
        {
            lock (_locker)
            {
                if (!_disposed)
                {
                    this.TimerCache?.RemoveTimer(this);
                    if (_timer != null)
                    {
                        _timer.Dispose();
                        _timer = null;
                    }
                    _disposed = true;
                }
            }
            GC.SuppressFinalize(this);
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
                    _timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
                    _timer.Change(0, Timeout.Infinite);
                }
            }
        }

        public bool Stop(int timeoutMS = 0)
        {
            lock (_locker)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(PeriodicTimer), "Cannot stop a disposed PeriodicTimer instance.");
                }
                if (_timer != null)
                {
                    return WaitForDispose(_timer, TimeSpan.FromMilliseconds(timeoutMS));
                }
                return true;
            }

            bool WaitForDispose(Timer timer, TimeSpan timeout)
            {
                if (timer != null)
                {
                    timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan); // 暂停派送新任务
                    if (timeout != TimeSpan.Zero)
                    {
                        ManualResetEvent waitHandle = new ManualResetEvent(false);
                        if (timer.Dispose(waitHandle) && !waitHandle.WaitOne((int)timeout.TotalMilliseconds))
                        {
                            timer = null;
                            return false;
                        }
                        waitHandle.Close();
                    }
                    else
                    {
                        timer.Dispose();
                    }
                }
                _timer = null;
                return true;
            }
        }

        private void Callback(object state)
        {
            if (TimerCallback == null)
            {
                return; // No subscribers, nothing to do
            }
            foreach (var del in this.TimerCallback?.GetInvocationList())
            {
                try
                {
                    del.DynamicInvoke();
                }
                catch (Exception ex)
                {
                    Task.Run(() =>
                    {
                        CallbackExceptionOccured?.Invoke(ex);
                    });
                }
            }
            try
            {
                _timer.Change(_periodMS, Timeout.Infinite);
            }
            catch (ObjectDisposedException)
            {
                ; // empty
            }
        }
    }
}