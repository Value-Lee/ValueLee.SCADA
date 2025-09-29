using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.DataAcquisition
{
    public class DataLogger : IDataLogger
    {
        private readonly ConcurrentDictionary<string, (Func<object> getter,bool saveDB)> _cache;

        public DataLogger()
        {
            _cache = new ConcurrentDictionary<string, (Func<object> getter, bool saveDB)> ();
        }

        public void Register(string id, (Func<object> getter, bool saveDB) dataItem)
        {
            
        }
    }
}