using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace Nuart.Modbus.TestApp.Views
{
    /// <summary>
    /// Interaction logic for SerialPortSettingView.xaml
    /// </summary>
    public partial class SerialPortSettingView : UserControl
    {
        public SerialPortSettingView()
        {
            InitializeComponent();
        }

        private void StackPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.Source as RadioButton).IsChecked != true)
            {
                if (Xceed.Wpf.Toolkit.MessageBox.Show("This have to restart the application.\nAre you sure to change modbus mode?", nameof(Nuart), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                    return;
                }
                e.Handled = true;
            }
        }
    }
}