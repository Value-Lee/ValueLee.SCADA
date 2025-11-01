using SCADA.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    public interface IPlcAccessor
    {
        int ReadTimeoutMS { get; }
        int WriteTimeoutMS { get; }

        long Read(params string[] names);
        long Read(bool allowFromCache, params string[] names);
        long Read(bool allowFromCache, int cacheExpiryTime, params string[] names);
         
        long Write(string name, object value);
        long Write(string name1, object value1, string name2, object value2);
        long Write(string name1, object value1, string name2, object value2, string name3, object value3);
        long Write(string name1, object value1, string name2, object value2, string name3, object value3, string name4, object value4);
        long Write(string name1, object value1, string name2, object value2, string name3, object value3, string name4, object value4, string name5, object value5);
        long Write(string name1, object value1, string name2, object value2, string name3, object value3, string name4, object value4, string name5, object value5, string name6, object value6);
        long Write(params (string name, object value)[] nameValues);

        bool CheckResult(int id, out OperationResult value);
        OperationResult WaitResult(int id);
    }
}