using System.Diagnostics;
using System.Windows;

namespace SCADA.Common.TestProject
{
    internal class MainWindowViewModel
    {
        private int _counter = 0;
        private bool _hasStarted = false;
        private Stopwatch _stopwatch = new Stopwatch();
        private PeriodicTimer _timer = new PeriodicTimer(1);

        public MainWindowViewModel()
        {
        }

        public void StartPeriodTimer()
        {
            if (!_hasStarted)
            {
                if (MessageBox.Show("Start timer?", "Info", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
                _hasStarted = true;
                _timer.Callback += (token) =>
                {
                    _counter++;
                    Thread.Sleep(20);
                    System.IO.File.AppendAllText("D:\\log.txt", $"{_counter}\r\n");
                };
                _timer.Start();
                _stopwatch.Start();
            }
        }

        public void StopPeriodTimer()
        {
            _timer.Stop(1000);
            _stopwatch.Stop();
            MessageBox.Show($"Counter: {_counter}\r\n " +
                $"ElapsedMilliseconds: {_stopwatch.ElapsedMilliseconds}ms \r\n " +
                $"avg: {(_stopwatch.ElapsedMilliseconds / _counter)}");
        }
    }
}