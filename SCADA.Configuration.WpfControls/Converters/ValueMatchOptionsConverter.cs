using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueLee.Configuration.WpfControls.Converters
{
    internal class ValueMatchOptionsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string value = (string)values[0];
            ValueType type = (ValueType)values[1];
            string[] options = (string[])values[2];
            if (type == ValueType.Integer)
            {
                foreach (var option in options)
                {
                    if (Utility.TryParse2Long(option, out long @longOption) && Utility.TryParse2Long(value, out long @longValue))
                    {
                        if (@longValue == @longOption)
                            return option;
                    }
                }
            }
            else if (type == ValueType.Decimal)
            {
                foreach (var option in options)
                {
                    if (Utility.TryParse2Decimal(option, out decimal @decimalOption) && Utility.TryParse2Decimal(value, out decimal @decimalValue))
                    {
                        if (@decimalValue == @decimalOption)
                            return option;
                    }
                }
            }
            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}