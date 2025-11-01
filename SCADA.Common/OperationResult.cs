using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.Common
{
    public class OperationResult
    {
        public bool IsSuccess => string.IsNullOrWhiteSpace(ErrMsg) && Exception == null;
        public string ErrMsg { get; set; }
        public Exception Exception { get; set; }
    }

    public class OperationResult<T> : OperationResult
    {
        public T Data { get; set; }
    }
}