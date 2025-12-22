using System;
using System.Collections.Generic;
using System.Text;

namespace SCADA.PLCFramework
{
    internal struct WriteData
    {
        public long ID { get; set; }
        public IList<(string name, object value)> NameValuePairs { get; set; }
    }
}
