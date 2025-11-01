using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.Configuration.WpfControls
{
    public class ObservableConfigItem : INotifyPropertyChanged
    {
        private bool _isEnable;
        private string _setpoint = string.Empty;
        private string _value;

        private bool _xmlEnable;
        private bool _xmlVisible;
        private bool isVisible = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Description { get; set; }
        public string Display { get; set; }

        public bool IsEnable
        {
            get
            {
                if (XmlEnable == false)
                {
                    return false;
                }
                return _isEnable;
            }
            set
            {
                if (_isEnable != value)
                {
                    _isEnable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnable)));
                }
            }
        }

        public bool IsVisible
        {
            get
            {
                if (XmlVisible == false)
                {
                    return false;
                }
                return isVisible;
            }
            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
                }
            }
        }

        public decimal Max { get; set; }
        public decimal Min { get; set; }
        public string Name { get; set; }
        public string[] Options { get; set; }
        public string Path { get; set; }
        public bool Restart { get; set; }

        public string Setpoint
        {
            get { return _setpoint; }
            set { _setpoint = value; }
        }

        public ValueType Type { get; set; }

        public string Unit { get; set; }

        public Action<string> ValidationRule { get; set; }

        public string Value
        {
            get => _value; set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public bool XmlEnable
        {
            get => _xmlEnable; set
            {
                if (_xmlEnable != value)
                {
                    _xmlEnable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnable)));
                }
            }
        }

        public bool XmlVisible
        {
            get => _xmlVisible; set
            {
                if (_xmlVisible != value)
                {
                    _xmlVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
                }
            }
        }
    }
}