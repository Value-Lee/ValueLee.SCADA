using System;
using System.Globalization;
using System.Windows.Data;

namespace SCADA.Configuration.WpfControls.Converters
{
    internal class Value2TextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string value = (string)values[0];
            string[] options = (string[])values[1];
            ValueType valueType = (ValueType)values[2];
            if (valueType == ValueType.Boolean)
            {
                bool value2 = System.Convert.ToBoolean(value);
                if (options == null || options.Length == 0)
                {
                    return value2 ? "Yes" : "No";
                }
                else
                {
                    return value2 ? options[0] : options[1];
                }
            }

            //if(valueType == ValueType.Integer)
            //{
            //}
            else
            {
                return value.ToString();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}