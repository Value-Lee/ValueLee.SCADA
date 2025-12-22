using System.Configuration;
using System.Data;
using System.Windows;

namespace SCADA.UI_Test
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return new MainWindow();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ;
        }
    }

}
