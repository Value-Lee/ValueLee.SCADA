using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Nuart.Modbus.TestApp.Converters
{
    internal class StopBitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stopBits = (StopBits)value;
            if (stopBits == StopBits.None)
            {
                return "None";
            }
            else if (stopBits == StopBits.One)
            {
                return "1";
            }
            else if (stopBits == StopBits.OnePointFive)
            {
                return "1,5";
            }
            else if (stopBits == StopBits.Two)
            {
                return "2";
            }
            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stopBits = System.Convert.ToString(value, CultureInfo.InvariantCulture);
            if (stopBits == "None")
            {
                return StopBits.None;
            }
            else if (stopBits == "1")
            {
                return StopBits.One;
            }
            else if(stopBits == "1.5")
            {
                return StopBits.OnePointFive;
            }
            else if(stopBits == "2")
            {
                return StopBits.Two;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
