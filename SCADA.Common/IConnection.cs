using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.Common
{
    public interface IConnection
    {
        string Address { get; set; }
        OperationResult Connect();
        Task<OperationResult> ConnectAsync();
        OperationResult Disconnect();
        Task<OperationResult> DisconnectAsync();
    }
}
