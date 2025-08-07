using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuart.Modbus.TestApp.Controls
{
    public class IntTypeConverter : TypeConverter
    {
        public bool AllowHex { get; set; }
        public IntTypeConverter()
        {
            AllowHex = true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string v = value as string;
            if (string.IsNullOrEmpty(v))
            {
                throw new ArgumentException("", nameof(value));
            }
            v = new string(v.Trim().Where(x => !char.IsWhiteSpace(x)).ToArray());
            if (AllowHex)
            {
                if ((v.StartsWith("0x") || v.StartsWith("0X")))
                {
                    return System.Convert.ToInt32(v.TrimStart(['0', 'x', 'X']), 16);
                }
                else
                {
                    throw new ArgumentException("", nameof(value));
                }
            }
            else
            {
                return System.Convert.ChangeType(value, typeof(int),culture);
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (AllowHex)
            {
                return value.ToString() + $" (0X{System.Convert.ToInt32(value).ToString("X2")})";
            }
            else
            {
                return value.ToString();
            }
            
        }
    }
}
