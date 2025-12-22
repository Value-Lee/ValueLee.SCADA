using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SCADA.TimerFSM.Samples
{
    internal class MainWindowViewModel
    {
        public ObservableCollection<string> Top3Logs { get; set; } = new ObservableCollection<string>()
        {
            "Log 1",
            "Log 2",
            "Log 3"
        };

        public ObservableCollection<string> AllLogs { get; set; } = new ObservableCollection<string>()
        {
            "Log 1",
            "Log 2",
            "Log 3",
            "Log 4",
            "Log 5",
            "Log 6",
            "Log 7",
            "Log 8",
            "Log 9",
            "Log 10"

        };
    }

    public class LogModel
    {
        public LogModel(DateTime timestamp, LogLevel level, string message, string category)
        {
            Timestamp = timestamp;
            Level = level;
            Message = message;
            Category = category;
        }

        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string Category { get; set; }
    }

    public enum LogLevel
    {
        Info,
        Warn,
        Alarm
    }
}
