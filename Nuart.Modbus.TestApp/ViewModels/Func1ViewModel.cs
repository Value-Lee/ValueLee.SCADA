using NLog;
using Nuart.FireForgetModel;
using Nuart.RequestReplyModel;
using System.Windows.Threading;

namespace Nuart.Modbus.TestApp.ViewModels
{
    public class Func1ViewModel : BindableBase, INavigationAware
    {
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IModbusClient _modbusClient;
        private bool _isReadSuccess;
        private int _pollInterval = 100;
        private int _quantity;
        private int _quantitySetpoint;
        private int _readTimeout = 100;
        private int _slaveUnit;
        private int _slaveUnitSetpoint;
        private int _startAddr;
        private int _startAddrSetpoint;
        private Timer _timer;

        public Func1ViewModel(IModbusClient modbusClient)
        {
            _slaveUnit = 1;
            _startAddr = 0;
            _quantity = 10;
            AddrValuePairs = new AddrValuePairObservableCollection();
            _modbusClient = modbusClient;
        }

        public AddrValuePairObservableCollection AddrValuePairs { get; }

        public bool IsReadSuccess
        {
            get { return _isReadSuccess; }
            set { SetProperty(ref _isReadSuccess, value); }
        }

        public int QuantirySetpoint
        {
            get { return _quantitySetpoint; }
            set { SetProperty(ref _quantitySetpoint, value); }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { SetProperty(ref _quantity, value); }
        }

        public int SlaveUnit
        {
            get { return _slaveUnit; }
            set { SetProperty(ref _slaveUnit, value); }
        }

        public int SlaveUnitSetpoint
        {
            get { return _slaveUnitSetpoint; }
            set { SetProperty(ref _slaveUnitSetpoint, value); }
        }

        public int StartAddr
        {
            get { return _startAddr; }
            set { SetProperty(ref _startAddr, value); }
        }

        public int StartAddrSetpoint
        {
            get { return _startAddrSetpoint; }
            set { SetProperty(ref _startAddrSetpoint, value); }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // 销毁定时器
            Task.Run(StopTimer);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 重启定时器
            StartTimer();
        }

        #region Modify Modbus Communication Settings

        public void SetQuantity(string quantiry)
        {
            if (IsInputInteger(quantiry, out int result))
            {
                try
                {
                    ArgumentChecker.CheckRegisterQuantity01(result);
                    Quantity = result;
                }
                catch
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Invalid Quantity for Out of Range.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Invalid Quantity for Not Integer", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void SetSlaveUnit(string slaveUnit)
        {
            if (IsInputInteger(slaveUnit, out int result))
            {
                try
                {
                    ArgumentChecker.CheckSlaveUnit01(result);
                    SlaveUnit = result;
                }
                catch
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Invalid Slave Unit for Out of Range.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Invalid Slave Unit for Not Integer", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void SetStartAddr(string startAddr)
        {
            if (IsInputInteger(startAddr, out int result))
            {
                try
                {
                    ArgumentChecker.CheckRegisterAddr01(result);
                    StartAddr = result;
                }
                catch
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Invalid Start Addr for Out of Range.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Invalid Start Addr for Not Integer", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion Modify Modbus Communication Settings

        private void callback(object state)
        {
            int slaveUnit = SlaveUnit;
            int startAddr = StartAddr;
            int quantity = Quantity;
            var response = _modbusClient.FC01(slaveUnit, startAddr, quantity, _readTimeout);

            IsReadSuccess = response.IsSuccess;
            if (IsReadSuccess)
            {
                if (AddrValuePairs.Quantity == quantity && AddrValuePairs.SlaveUnit == slaveUnit && AddrValuePairs.StartAddr == startAddr)
                {
                    for (int i = 0; i < quantity; i++)
                    {
                        AddrValuePairs[i].Value = response.Data[i];
                    }
                }
                else
                {
                    AddrValuePairs.Clear();
                    for (int i = 0; i < quantity; i++)
                    {
                        _dispatcher?.Invoke(() =>
                        {
                            AddrValuePairs.Add(new AddrValuePair()
                            {
                                Addr = startAddr + i,
                                Value = response.Data[i]
                            });
                        });
                    }
                }
                AddrValuePairs.Quantity = quantity;
                AddrValuePairs.SlaveUnit = slaveUnit;
                AddrValuePairs.StartAddr = startAddr;
            }
            else
            {
                AddrValuePairs.Clear();
                AddrValuePairs.SlaveUnit = -1;
                AddrValuePairs.StartAddr = -1;
                AddrValuePairs.Quantity = -1;
                var client = (ISerialInterface)_modbusClient;
                if (client.RecvBuffLength > 0)
                {
                    client.Reset();
                }
            }

            _timer.Change(TimeSpan.FromMilliseconds(_pollInterval), Timeout.InfiniteTimeSpan);
        }

        private bool IsInputInteger(string inputText, out int result)
        {
            result = 0;
            if (string.IsNullOrEmpty(inputText))
            {
                return false;
            }
            inputText = new string(inputText.Trim().Where(x => !char.IsWhiteSpace(x)).ToArray());
            if (inputText.StartsWith("0x") || inputText.StartsWith("0X"))
            {
                try
                {
                    result = System.Convert.ToInt32(inputText.Substring(2), 16);
                    return true;
                }
                catch { return false; }
            }
            else
            {
                return int.TryParse(inputText, out result);
            }
        }

        private void StartTimer()
        {
            _timer = new Timer(callback);
            _timer.Change(TimeSpan.FromMilliseconds(_pollInterval), Timeout.InfiniteTimeSpan);
        }

        private void StopTimer()
        {
            var success = _timer.WaitForDispose(TimeSpan.FromMilliseconds(_readTimeout * 1.5));

            if (!success)
            {
                _logger.Error("***Failed to destory timer for timeout***");
            }
        }
    }
}