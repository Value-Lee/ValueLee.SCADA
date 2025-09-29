using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.DataAcquisition
{
    public interface IDataLogger
    {
        void Register(string id, (Func<object> getter, bool saveDB) dataItem);
        object Read(string id);

        void StartLog();
        void StopLog();
        void SetInterval(int interval);
    }
}
