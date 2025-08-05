using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ValueLee.Common
{
    public sealed class PeriodicTimerManager : IDisposable
    {
        private readonly int _defaultPeriodMS;

        private readonly object _periodsLocker;

        private readonly List<(string id, int periodMS)> _periods;
        private readonly List<(string id, WeakReference timer)> _timers;
        private readonly object _timersLocker;

        public PeriodicTimerManager()
        {
            _timers = new List<(string id, WeakReference timer)>();
            _periods = new List<(string id, int periodMS)>();

            _periodsLocker = ((ICollection)_periods).SyncRoot;
            _timersLocker = ((ICollection)_timers).SyncRoot;

            _defaultPeriodMS = 100;
        }

        public void Dispose()
        {
            _periods.Clear();
            for (int i = _timers.Count - 1; i >= 0; i--)
            {
                (_timers[i].timer.Target as PeriodicTimer)?.Dispose();
            }
            _timers.Clear();
        }

        //public void StopAll()
        //{
        //    lock (_timersLocker)
        //    {
        //        foreach (var timer in _timers)
        //        {
        //            if (timer.timer.Target is PeriodicTimer periodicTimer)
        //            {
        //                periodicTimer.Dispose();
        //            }
        //        }
        //        _timers.Clear();
        //    }
        //}

        public PeriodicTimer GetTimer(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id), "ID cannot be null.");
            }
            lock (_timersLocker)
            {
                var target = _timers.FirstOrDefault(t => t.id == id);
                if (target != default((string, WeakReference)))
                {
                    var existingTimer = target.timer.Target as PeriodicTimer;
                    if (existingTimer != null)
                    {
                        return existingTimer;
                    }
                    else
                    {
                        var timer = new PeriodicTimer(GetPeriod(id));
                        timer.TimerManager = this;
                        _timers.Add((id, new WeakReference(timer)));
                        return timer;
                    }
                }
                else
                {
                    var timer = new PeriodicTimer(GetPeriod(id));
                    timer.TimerManager = this;
                    _timers.Add((id, new WeakReference(timer)));
                    return timer;
                }
            }
        }

        public PeriodicTimerManager PresetPeriod(string id, int periodMS)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id), "ID cannot be null.");
            }
            if (periodMS <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(periodMS), "Period must be greater than zero.");
            }
            lock (_periodsLocker)
            {
                var target = _periods.FirstOrDefault(p => p.id == id);
                if (target == default((string, int)))
                {
                    _periods.Add((id, periodMS));
                }
                else
                {
                    throw new DuplicateNameException($"A period with ID '{id}' already exists.");
                }
                return this;
            }
        }

        internal void RemoveTimer(PeriodicTimer timer)
        {
            lock (_timersLocker)
            {
                var target = _timers.FirstOrDefault(t => t.timer.Target == timer);
                if (target != default((string, WeakReference)))
                {
                    _timers.Remove(target);
                }
            }
        }

        private int GetPeriod(string id)
        {
            lock (_periodsLocker)
            {
                var target = _periods.FirstOrDefault(p => p.id == id);
                if (target == default((string, int)))
                {
                    return _defaultPeriodMS;
                }
                return target.periodMS;
            }
        }
    }
}