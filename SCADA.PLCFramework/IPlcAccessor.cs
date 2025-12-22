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
        // 返回时间戳
        ReadResult Read(string name);
        ReadResult Read(string name1, string name2);
        ReadResult Read(string name1, string name2, string name3);
        ReadResult Read(string name1, string name2, string name3, string name4);
        ReadResult Read(string name1, string name2, string name3, string name4, string name5);
        ReadResult Read(string name1, string name2, string name3, string name4, string name5, string name6);
        ReadResult Read(params string[] names);

        // write写到队列中，返回写入队列的id
        // 查询id是否还在字典中,如果在，说明还没写到PLC，返回false
        long Write(string name, object value);
        long Write(string name1, object value1, string name2, object value2);
        long Write(string name1, object value1, string name2, object value2, string name3, object value3);
        long Write(string name1, object value1, string name2, object value2, string name3, object value3, string name4, object value4);
        long Write(string name1, object value1, string name2, object value2, string name3, object value3, string name4, object value4, string name5, object value5);
        long Write(string name1, object value1, string name2, object value2, string name3, object value3, string name4, object value4, string name5, object value5, string name6, object value6);
        long Write(params (string name, object value)[] nameValues);

        bool WriteCompleted(long id);
    }
}