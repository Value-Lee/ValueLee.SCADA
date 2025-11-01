using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    internal class CommandInfo
    {
        public int ID { get; set; }
        public IList<string> Names { get; set; }
        public IList<object> Values { get; set; }
        public Operation Operation { get; set; }
        public bool AllowFromCache { get; set; }
        public int CacheExpiryTime { get; set; }
    }
}
