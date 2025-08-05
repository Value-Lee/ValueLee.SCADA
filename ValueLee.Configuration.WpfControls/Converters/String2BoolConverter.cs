using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueLee.Configuration.WpfControls.Converters
{
    internal class String2BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToBoolean(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}