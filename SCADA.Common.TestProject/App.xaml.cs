using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace ValueLee.Common.TestProject;

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
        base.OnStartup(e);

        SCADA.Common.PeriodicTimer timer = new SCADA.Common.PeriodicTimer(500); // Period is 500ms.
        // SCADA.Common.PeriodicTimer timer = new SCADA.Common.PeriodicTimer(); // Default period is 100ms.

        /* Callback can add multiple tasks.
         *
         */
        timer.Callback += (cancellationToken) =>
        {
            // 建议使用同步方法，因为方法本身就是在线程池线程执行。使用异步方法，则会在线程池线程内再创建新的线程池线程，效率较低。
            // 使用异步方法，则会出现上次任务还未结束，而下次任务又开始的情况，可能会引起并发问题。
            // 若不得不使用异步方法，可以考虑使用GetAwaiter().GetResult()转成同步，而不要使用Wait()或Result，以避免死锁。
            // 尽量使用CancellationToken来支持取消操作。因为Stop()时会触发取消操作，可以更快的等待所有任务全部结束。
            System.IO.File.AppendAllTextAsync("C:\\log.txt", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}\r\n", cancellationToken).GetAwaiter().GetResult();
        };

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
        // CallbackExceptionOccured被同步执行，如果不想异常操作影响周期，可以在事件处理程序内使用Task.Run()来异步处理异常。
        timer.CallbackExceptionOccured += (ex) =>
        {
            System.IO.File.AppendAllTextAsync("D:\\log.txt", $"Exception: {ex.ToString()}\r\n");
        };

        // True,Exception in one subscriber will not affect other subscribers.
        // False,Exception in one subscriber will prevent other subscribers from executing.
        timer.ContinueOtherTasksWhenExceptionOccured = false;

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

        // Don't forget to dispose the timer when it is no longer needed.
        timer.Dispose();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}