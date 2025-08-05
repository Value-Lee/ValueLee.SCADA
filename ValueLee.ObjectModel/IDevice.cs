using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValueLee.ObjectModel
{
    public interface IDevice : IModular, IScript, IDataCache
    {
        void Initialize();
        void Terminate();
        void Monitor();
        void Reset();
    }
}
