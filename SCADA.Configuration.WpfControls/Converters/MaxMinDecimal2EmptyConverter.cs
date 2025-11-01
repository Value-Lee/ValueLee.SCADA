using System;
using System.Globalization;
using System.Windows.Data;

namespace SCADA.Configuration.WpfControls.Converters
{
    internal class MaxMinDecimal2EmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal d = System.Convert.ToDecimal(value);
            if (d == decimal.MaxValue || d == decimal.MinValue)
            {
                return string.Empty;
            }
            else
            {
                return d.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}