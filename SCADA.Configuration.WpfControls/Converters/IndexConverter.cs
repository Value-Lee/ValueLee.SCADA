using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace SCADA.Configuration.WpfControls.Converters
{
    public class IndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var itemsControl = values[0] as ItemsControl;
            var dataContext = values[1] as ObservableConfigItem;
            return System.Convert.ToString(itemsControl.Items.Cast<ObservableConfigItem>().Where(x=>x.IsVisible == true).ToList().IndexOf(dataContext) + 1);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
