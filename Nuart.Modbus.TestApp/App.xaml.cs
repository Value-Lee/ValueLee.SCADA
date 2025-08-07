using DryIoc;
using Nuart.Modbus.TestApp.ViewModels;
using Nuart.Modbus.TestApp.Views;
using System.Windows;

namespace Nuart.Modbus.TestApp
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

        protected override void OnStartup(StartupEventArgs e)
        {
            Xceed.Wpf.Toolkit.Licenser.LicenseKey = "WTK46-DFFYR-0R9GW-0R1A";
            base.OnStartup(e);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<MainWindowViewModel>();
            containerRegistry.Register<Func1ViewModel>();
            containerRegistry.Register<Func2ViewModel>();
            containerRegistry.Register<Func3ViewModel>();
            containerRegistry.Register<Func4ViewModel>();
            containerRegistry.Register<Func5ViewModel>();
            containerRegistry.Register<Func6ViewModel>();
            containerRegistry.Register<Func15ViewModel>();
            containerRegistry.Register<Func16ViewModel>();
            containerRegistry.Register<Func23ViewModel>();

            containerRegistry.RegisterForNavigation<Func1View>("Func1");
            containerRegistry.RegisterForNavigation<Func2View>("Func2");
            containerRegistry.RegisterForNavigation<Func3View>("Func3");
            containerRegistry.RegisterForNavigation<Func4View>("Func4");
            containerRegistry.RegisterForNavigation<Func5View>("Func5");
            containerRegistry.RegisterForNavigation<Func6View>("Func6");
            containerRegistry.RegisterForNavigation<Func15View>("Func15");
            containerRegistry.RegisterForNavigation<Func16View>("Func16");
            containerRegistry.RegisterForNavigation<Func23View>("Func23");

            Container.Resolve<IRegionManager>().RegisterViewWithRegion<SerialPortSettingView>("HeaderRegion");
            Container.Resolve<IRegionManager>().RegisterViewWithRegion<FuncCodeMenuView>("MenuRegion");

            containerRegistry.GetContainer().Register<IModbusClient>(Reuse.Singleton, Made.Of(() => new ModbusRtuClient("COM1")));
            //containerRegistry.GetContainer().Unregister<IModbusClient>();
            //containerRegistry.RegisterSingleton<IModbusClient, ModbusAsciiClient>();
        }
    }
}