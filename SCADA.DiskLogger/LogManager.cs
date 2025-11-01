using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace SCADA.DiskLogger
{
    public class LogManager
    {
        public static Logger GetCurrentClassLogger()
        {
            return NLog.LogManager.GetCurrentClassLogger();
        }

        public static Logger GetLogger(string name)
        {
            return NLog.LogManager.GetLogger(name); ;
        }

        public LogManager()
        {
            
        }
    }
}
