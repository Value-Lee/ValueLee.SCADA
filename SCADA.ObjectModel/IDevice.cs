using SCADA.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.ObjectModel
{
    public interface IDevice
    {
        void Initialize();
        void Monitor();
        void Reset();
        void Terminate();
    }
}
