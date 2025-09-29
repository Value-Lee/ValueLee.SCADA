using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SCADA.UART.Framework
{
    internal class RequestInfo<TCharOrByte> where TCharOrByte : struct, IConvertible, IComparable
    {
        private readonly Stopwatch _stopwatch;
        private bool _hasBeenSent;
        private int _retryCount;

        public RequestInfo(IList<TCharOrByte> contents, string commandName, int timeoutMS, int retryLimit, int id)
        {
            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }
            if (contents.Count == 0)
            {
                throw new ArgumentException("Contents cannot be empty.", nameof(contents));
            }
            if (commandName == null)
            {
                throw new ArgumentNullException(nameof(commandName));
            }
            if (timeoutMS <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutMS), "Timeout must be greater than zero.");
            }
            if (retryLimit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryLimit), "Retry limit cannot be negative.");
            }
            CommandName = commandName;
            Contents = contents;
            TimeoutMS = timeoutMS;
            RetryLimit = retryLimit + 1;
            ID = id;
            _retryCount = 0;
            _hasBeenSent = false;
            _stopwatch = new Stopwatch();
        }

        public bool CanRetry => _retryCount < RetryLimit;
        public string CommandName { get; set; }
        public IList<TCharOrByte> Contents { get; set; }
        public bool HasBeenSent => _hasBeenSent;
        public int ID { get; set; }
        public bool IsTimeout => _stopwatch.ElapsedMilliseconds > TimeoutMS;
        public int RetryLimit { get; set; }
        public int TimeoutMS { get; set; }

        public int IncrementRetry()
        {
            return ++_retryCount;
        }

        public void RestartTimer()
        {
            _stopwatch.Restart();
        }

        public void SetSendStatus(bool hasBeenSent)
        {
            _hasBeenSent = hasBeenSent;
        }
    }
}