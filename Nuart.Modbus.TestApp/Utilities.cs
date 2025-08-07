using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuart.Modbus.TestApp
{
    public static class Utilities
    {
        public static bool WaitForDispose(this Timer timer, TimeSpan timeout)
        {
            timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan); // 暂停派送新任务
            if (timeout != TimeSpan.Zero)
            {
                ManualResetEvent waitHandle = new ManualResetEvent(false);
                if (timer.Dispose(waitHandle) && !waitHandle.WaitOne((int)timeout.TotalMilliseconds))
                {
                    return false;
                }
                waitHandle.Close();
            }
            else
            {
                timer.Dispose();
            }
            return true;
        }
    }
}
