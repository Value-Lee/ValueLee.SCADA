## PeriodicTimer

PeriodicTimer是基于System.Threading.Timer封装而成的定时器，具有以下特点：

- 不会发生回调重入。任务在线程池线程上被执行，上一次任务未完成，不会启动下一次任务。

  假设任务耗时是1S，周期是2S，则任务实际每3S被执行一次。

- 可以随时启动，暂停定时器，修改周期，这些操作都是线程安全的。

- 暂停定时器时，线程池中可能还有正在执行的定时任务，或者已经在线程池外排队尚未来得及执行的任务。Stop(大于0的参数)不再派发新的定时任务到线程池，且等待所有已经派发过的任务全部结束后返回，而Stop(0)只是不会再派发新任务但不等待立刻返回。在程序开发中按需选择哪种暂停方式。

```c#
SCADA.Common.PeriodicTimer timer = new SCADA.Common.PeriodicTimer(500); // Period is 500ms.
// SCADA.Common.PeriodicTimer timer = new SCADA.Common.PeriodicTimer(); // Default period is 100ms.

/* Callback can add multiple tasks.
 * 建议使用同步方法，因为方法本身就是在线程池线程执行。使用异步方法，则会在线程池线程内再创建新的线程池线程，效率较低。
 * 使用异步方法，则会出现上次任务还未结束，而下次任务又开始的情况，可能会引起并发问题。
 * 若不得不使用异步方法，可以考虑使用GetAwaiter().GetResult()转成同步，而不要使用Wait()或Result，以避免死锁。
 * 尽量使用带CancellationToken参数的方法，因为Stop()可以触发取消，可以更快的等待到所有任务全部结束。
 */

// Add Task 1
timer.Callback += (cancellationToken) =>
{
    System.IO.File.AppendAllTextAsync("C:\\log.txt", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}\r\n", cancellationToken).GetAwaiter().GetResult();
};

// Add Task2
timer.Callback += async (token) =>
{
    try
    {
        HttpClient client = new HttpClient();
        await client.GetStringAsync("www.google.com", token);
    }
    catch
    {
    }
};

// 当某个任务抛出异常时，会触发CallbackExceptionOccured事件。
// CallbackExceptionOccured被同步执行，如果不想异常处理影响周期，可以在事件处理程序内使用Task.Run()来异步处理异常。
timer.CallbackExceptionOccured += (ex) =>
{
    System.IO.File.AppendAllTextAsync("D:\\log.txt", $"Exception: {ex.ToString()}\r\n");
};

// True,Exception in one task will not affect other tasks.
// False,Exception in one task will prevent other tasks from executing.
timer.ContinueOtherTasksWhenExceptionOccured = false; // default value is true

// Start the timer.
timer.Start();

// Change period at any time.
timer.ChangePeriod(500); // Change period to 500ms.

// Stop the timer.
// Stop表示不再触发新的任务，如果timeout>0，会等待所有已触发的任务执行完毕后才返回，返回true，表示timeout毫秒内，所有任务结束，false，则还未结束。
// timeout = 0,表示不再触发新的任务也不等待已触发的任务全部结束，立即返回。
var success = timer.Stop(2000);

if (!success)
{
    Debug.WriteLine("WARN: Failed to stop the timer within the timeout.");
}

// You can restart timer again
timer.Start();

// Don't forget to dispose the timer when it is no longer needed.
timer.Dispose();
```

## TODO

- ~~PeriodicTimer要支持CancellationToken，以尽快完成Stop。~~
