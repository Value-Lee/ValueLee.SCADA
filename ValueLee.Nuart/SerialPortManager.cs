using Nuart;
using Nuart.RequestReplyModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValueLee.Common;

namespace ValueLee.Nuart
{
    public class SerialPortManager : IDisposable
    {
        #region Events

        public event Action<SerialEventArgs<byte[]>> CompletedFrameReceived;

        public event Action<SerialEventArgs<byte[]>> DataRead;

        public event Action<SerialEventArgs<byte[]>> DataSent;

        public event Action<SerialEventArgs<Exception>> TimedDataReadingJobThrowException;

        #endregion Events

        private readonly Dictionary<string, ISerialPort> _serialPorts;
        private readonly object _syncRoot;
        private readonly int _timerPeriod = 20;
        private readonly List<PeriodicTimer> _timers;

        public SerialPortManager()
        {
            _timers = new List<PeriodicTimer>();
            _serialPorts = new Dictionary<string, ISerialPort>();
            _syncRoot = new object();
        }

        public int PeriodicTimerCountThreshold { get; set; } = 10;

        public void Dispose()
        {
        }

        private PeriodicTimer GetTimer()
        {
            var target = _timers.FirstOrDefault(t => t.Callback == null || t.Callback.GetInvocationList().Length < PeriodicTimerCountThreshold);
            if (target != null)
            {
                return target;
            }
            else
            {
                var newTimer = new PeriodicTimer(_timerPeriod);
                newTimer.CallbackExceptionOccured += Timer_CallbackExceptionOccured;
                _timers.Add(newTimer);
                return newTimer;
            }
        }

        #region Create

        public ISerialPort Create<TReceiveFilter>(string portName, int baudRate, Parity parity, StopBits stopBits, int dataBits, bool rtsEnable, Handshake handshake, bool exclusiveTimer = false) where TReceiveFilter : IReceiveFilter, new()
        {
            lock (_syncRoot)
            {
                if (_serialPorts.ContainsKey(portName))
                {
                    throw new InvalidOperationException($"Serial port '{portName}' is already created.");
                }

                var serialPort = new SerialPort<TReceiveFilter>(
                     portName, baudRate, parity, stopBits, dataBits, rtsEnable, handshake);
                _serialPorts[portName] = serialPort;
                serialPort.CompletedFrameReceived += SerialPort_CompletedFrameReceived;
                serialPort.DataRead += SerialPort_DataRead;
                serialPort.DataSent += SerialPort_DataSent;

                if (exclusiveTimer)
                {
                    var timer = new PeriodicTimer(_timerPeriod);
                    timer.Callback += ((ITimer)serialPort).RecvData;
                    timer.CallbackExceptionOccured += Timer_CallbackExceptionOccured;
                    _timers.Add(timer);
                    ((ITimer)serialPort).PeriodicTimer = timer;
                    timer.Start();
                }
                else
                {
                    var timer = GetTimer();
                    timer.Callback += ((ITimer)serialPort).RecvData;
                    ((ITimer)serialPort).PeriodicTimer = timer;
                    timer.Start();
                }

                return serialPort;
            }
        }

        public ISerialPort Create<TReceiveFilter>(string portName, int baudRate, Parity parity, StopBits stopBits, int dataBits, bool exclusiveTimer = false) where TReceiveFilter : IReceiveFilter, new()
        {
            return Create<TReceiveFilter>(portName, baudRate, parity, stopBits, dataBits, false, Handshake.None, exclusiveTimer);
        }

        public ISerialPort Create<TReceiveFilter>(string portName, int baudRate, Parity parity, StopBits stopBits, bool exclusiveTimer = false) where TReceiveFilter : IReceiveFilter, new()
        {
            return Create<TReceiveFilter>(portName, baudRate, parity, stopBits, 8, exclusiveTimer);
        }

        public ISerialPort Create<TReceiveFilter>(string portName, int baudRate, bool exclusiveTimer = false) where TReceiveFilter : IReceiveFilter, new()
        {
            return Create<TReceiveFilter>(portName, baudRate, Parity.None, StopBits.One, exclusiveTimer);
        }

        public ISerialPort Create<TReceiveFilter>(string portName, bool exclusiveTimer = false) where TReceiveFilter : IReceiveFilter, new()
        {
            return Create<TReceiveFilter>(portName, 9600, exclusiveTimer);
        }

        private void SerialPort_CompletedFrameReceived(SerialEventArgs<byte[]> obj)
        {
            Task.Factory.StartNew(() =>
            {
                CompletedFrameReceived?.Invoke(obj);
            });
        }

        private void SerialPort_DataRead(SerialEventArgs<byte[]> obj)
        {
            Task.Factory.StartNew(() =>
            {
                DataRead?.Invoke(obj);
            });
        }

        private void SerialPort_DataSent(SerialEventArgs<byte[]> args)
        {
            Task.Factory.StartNew(() =>
            {
                DataSent?.Invoke(args);
            });
        }

        private void Timer_CallbackExceptionOccured()
        {
            Task.Factory.StartNew(() =>
            {
                TimedDataReadingJobThrowException?.Invoke(obj);
            });
        }

        #endregion Create

        #region Release

        public void Release(ISerialPort serialPort)
        {
            lock (_syncRoot)
            {
                if (serialPort != null)
                {
                    var serialPort2 = serialPort as ITimer;
                    serialPort2.PeriodicTimer.Callback -= serialPort2.RecvData;

                    if (_serialPorts.ContainsKey(serialPort.PortName))
                    {
                        _serialPorts.Remove(serialPort.PortName);
                        serialPort.Dispose();
                    }
                    if (serialPort2.PeriodicTimer.Callback == null)
                    {
                        serialPort2.PeriodicTimer.Dispose();
                        _timers.Remove(serialPort2.PeriodicTimer);
                    }
                }
            }
        }

        public void Release(string portName)
        {
            if (_serialPorts.TryGetValue(portName, out var serialPort))
            {
                Release(serialPort);
            }
        }

        public void ReleaseAll()
        {
        }

        #endregion Release
    }
}