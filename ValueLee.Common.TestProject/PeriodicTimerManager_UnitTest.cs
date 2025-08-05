namespace ValueLee.Common.TestProject
{
    public class PeriodicTimerManager_UnitTest
    {
        private int i = 0;

        [Fact]
        public void Test1()
        {
            {
                using var timerManager = new PeriodicTimerManager();
                timerManager.PresetPeriod("testTimer", 100);
                var timer = timerManager.GetTimer("testTimer");
                timer.TimerCallback += Timer_TimerCallback;
                timer.Start();
                Thread.Sleep(1000);
                Assert.True(i > 5);
                timer.Stop(1000);
            }

            {
                var timerManager = new PeriodicTimerManager();
                timerManager.PresetPeriod("Timer1", 100);
                timerManager.PresetPeriod("Timer2", 100);
                timerManager.PresetPeriod("Timer3", 100);
                var timer1 = timerManager.GetTimer("Timer1");
                var timer2 = timerManager.GetTimer("Timer2");
                var timer3 = timerManager.GetTimer("Timer3");
                timer1.Start();
                timer2.Start();
                timer3.Start();
                timerManager.Dispose();
                Type timerManagerType = timerManager.GetType();
                var _timersFieldInfo = timerManagerType.GetField("_timers", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var _timers = _timersFieldInfo.GetValue(timerManager);
                Assert.Equal(0, (((List<(string id, WeakReference timer)>)_timers).Count));

                var _periodsFieldInfo = timerManagerType.GetField("_periods", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var _periods = _periodsFieldInfo.GetValue(timerManager);
                Assert.Equal(0, (((List<(string id, int periodMS)>)_periods).Count));
            }
        }

        private void Timer_TimerCallback()
        {
            i++;
        }
    }
}