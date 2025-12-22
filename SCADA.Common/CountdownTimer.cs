using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.Common
{
    public class CountdownTimer
    {
        private readonly Stopwatch _stopwatch;
        private long _threshold;
        public CountdownTimer(long threshold)
        {
            if (threshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold can not be negative.");
            }
            _stopwatch = new Stopwatch();
            _threshold = threshold;
        }
        public CountdownTimer(TimeSpan threshold) : this(Convert.ToInt64(threshold.TotalMilliseconds))
        {
        }
        public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;
        public TimeSpan ElapsedTime => TimeSpan.FromMilliseconds(_stopwatch.ElapsedMilliseconds);
        public bool IsIdle => !_stopwatch.IsRunning;
        public bool IsTimeout => _stopwatch.ElapsedMilliseconds >= _threshold;
        public void Start(TimeSpan? threshold = null)
        {
            if (threshold != null)
            {
                _threshold = Convert.ToInt64(threshold.Value.TotalMilliseconds);
            }
            _stopwatch.Restart();
        }
        public void Start(long? thresholdMS = null)
        {
            if (thresholdMS != null)
            {
                if (thresholdMS.Value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(thresholdMS), "Threshold can not be negative.");
                }
                _threshold = thresholdMS.Value;
            }
            _stopwatch.Restart();
        }
        public void Stop()
        {
            _stopwatch.Stop();
        }
    }
}