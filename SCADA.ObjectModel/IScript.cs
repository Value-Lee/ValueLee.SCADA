using SCADA.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.ObjectModel
{
    public delegate OperationResult ScriptMethodDelegate(params object[] args);

    public interface IScript : IModular
    {
        void Execute(string id, string methodName, params object[] args);

        void Execute(string module, string name, string methodName, params object[] args);

        void Register(string id, string methodName, ScriptMethodDelegate @delegate);

        void Register(string module, string name, string methodName, ScriptMethodDelegate @delegate);
    }
}