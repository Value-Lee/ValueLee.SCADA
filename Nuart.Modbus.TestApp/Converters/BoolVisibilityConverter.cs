using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Nuart.Modbus.TestApp.Converters
{
    internal class BoolVisibilityConverter : IValueConverter
    {
        public Visibility TrueVisibility { get; set; } = Visibility.Visible;
        public Visibility FalseVisibility { get; set; } = Visibility.Hidden;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool v = System.Convert.ToBoolean(value);
            return v ? TrueVisibility : FalseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
