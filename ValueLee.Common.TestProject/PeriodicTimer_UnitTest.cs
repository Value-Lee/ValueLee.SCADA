using System;
using System.Collections.Generic;
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
            using var timer = new PeriodicTimer();
            timer.TimerCallback += Timer_TimerCallback;
        }

        private void Timer_TimerCallback()
        {
            throw new NotImplementedException();
        }
    }
}
