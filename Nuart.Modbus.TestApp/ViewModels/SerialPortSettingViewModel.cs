using System.IO;
using System.IO.Ports;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nuart.Modbus.TestApp.ViewModels
{
    internal class SerialPortSettingViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private int _baudRate;
        private int _dataBits;
        private bool _isAscMode;
        private bool _isRtuMode;
        private Parity _parity;
        private string _portName;
        private StopBits _stopBits;

        public SerialPortSettingViewModel(IEventAggregator eventAggregator)
        {
            _portName = "COM1";
            _baudRate = 9600;
            _dataBits = 8;
            _parity = Parity.None;
            _stopBits = StopBits.One;
            _eventAggregator = eventAggregator;
            string configFileName = Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName),"AppConfig.json");
           
            
            _isRtuMode = true;
        }

        public int BaudRate { get => _baudRate; set => SetProperty(ref _baudRate, value); }

        public IEnumerable<int> BaudRateOptions { get; } = new int[]
        {
            2400,4800,9600,19200,38400,57600,115200
        }.Select(x => x);

        public int DataBits { get => _dataBits; set => SetProperty(ref _dataBits, value); }

        public bool IsAscMode
        {
            get { return _isAscMode; }
            set { SetProperty(ref _isAscMode, value); }
        }

        public bool IsRtuMode
        {
            get { return _isRtuMode; }
            set { SetProperty(ref _isRtuMode, value); }
        }

        public Parity Parity { get => _parity; set => SetProperty(ref _parity, value); }

        public IEnumerable<string> ParityOptions { get; } = new
            System.IO.Ports.Parity[]
        {
            System.IO.Ports.Parity.None,
            System.IO.Ports.Parity.Odd,
            System.IO.Ports.Parity.Even,
            System.IO.Ports.Parity.Mark,
            System.IO.Ports.Parity.Space,
        }.Select(x => x.ToString());

        public string PortName { get => _portName; set => SetProperty(ref _portName, value); }
        public IEnumerable<string> PortOptions { get; } = Enumerable.Range(1, 200).Select(x => "COM" + x.ToString());
        public StopBits StopBits { get => _stopBits; set => SetProperty(ref _stopBits, value); }

        public IEnumerable<string> StopBitsOptions { get; } = new
            string[]
        {
            "None","1","1.5","2"
        }.Select(x => x.ToString());
    }
}