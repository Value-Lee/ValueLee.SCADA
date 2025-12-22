using SCADA.TimerFSM.Samples.TrafficLightSample;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SCADA.TimerFSM.Samples
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            TrafficLight trafficLight = new TrafficLight();
            trafficLight.PostMsg(TrafficLight.TrafficLightCommand.Green);
        }
    }

}
