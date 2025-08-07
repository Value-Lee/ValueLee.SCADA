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
    internal class ParityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parity = (Parity)value;
            return parity.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parity = System.Convert.ToString(value);
            if (parity == Parity.None.ToString())
            {
                return Parity.None;
            }
            else if (parity == Parity.Even.ToString())
            {
                return Parity.Even;
            }
            else if (parity == Parity.Odd.ToString())
            {
                return Parity.Odd;
            }
            else if (parity == Parity.Space.ToString())
            {
                return Parity.Space;
            }
            else if (parity == Parity.Mark.ToString())
            {
                return Parity.Mark;
            }
            throw new ArgumentException();
        }
    }
}