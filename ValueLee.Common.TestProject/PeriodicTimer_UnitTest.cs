using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValueLee.Common.TestProject
{
    public class PeriodicTimer_UnitTest
    {
        [Fact]
        public void Test1()
        {
            using var timer = new PeriodicTimer(50);
            timer.TimerCallback += Timer_TimerCallback;
            timer.Start();
            Thread.Sleep(1000);
            var timeout = !timer.Stop(1000);
            Assert.False(timeout, "Timer did not stop within the expected time frame.");
        }

        private void Timer_TimerCallback()
        {
           Debug.WriteLine("INFO: Timer callback executed at: " + DateTime.Now);
        }
    }
}
