using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.PLCFramework
{
    public readonly struct ReadResult<T>
    {
        public ReadResult(long timestamp, T value)
        {
            Timestamp = timestamp;
            Value = value;
        }

        public long Timestamp { get; }
        public T Value { get; }
    }

    public readonly struct ReadResult<T1,T2>
    {
        public ReadResult(long timestamp, T1 value1, T2 value2)
        {
            Timestamp = timestamp;
            Value1 = value1;
            Value2 = value2;
        }

        public long Timestamp { get; }
        public T1 Value1 { get; }
        public T2 Value2 { get; }
    }

    public readonly struct ReadResult<T1, T2,T3>
    {
        public ReadResult(long timestamp, T1 value1, T2 value2, T3 value3)
        {
            Timestamp = timestamp;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }

        public long Timestamp { get; }
        public T1 Value1{ get; }
        public T2 Value2{ get; }
        public T3 Value3 { get; }
    }

    public readonly struct ReadResult<T1, T2, T3,T4>
    {
        public ReadResult(long timestamp, T1 value1, T2 value2, T3 value3, T4 value4)
        {
            Timestamp = timestamp;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
        }

        public long Timestamp { get;  }
        public T1 Value1{ get; }
        public T2 Value2{ get; }
        public T3 Value3{ get; }
        public T4 Value4 { get; }
    }

    public readonly struct ReadResult<T1, T2, T3, T4,T5>
    {
        public ReadResult(long timestamp, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            Timestamp = timestamp;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
        }

        public long Timestamp { get; }
        public T1 Value1{ get; }
        public T2 Value2{ get; }
        public T3 Value3{ get; }
        public T4 Value4{ get; }
        public T5 Value5 { get; }
    }

    public readonly struct ReadResult<T1, T2, T3, T4, T5,T6>
    {
        public ReadResult(long timestamp, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
        {
            Timestamp = timestamp;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
            Value6 = value6;
        }

        public long Timestamp { get;  }
        public T1 Value1{ get; }
        public T2 Value2{ get; }
        public T3 Value3{ get; }
        public T4 Value4{ get; }
        public T5 Value5{ get; }
        public T6 Value6 { get; }
    }

    public readonly struct ReadResult
    {
        public ReadResult(long timestamp, IList<object> values)
        {
            Timestamp = timestamp;
            Values = values;
        }

        public long Timestamp { get; }
        public IList<object> Values { get;  }

        public T GetValue<T>(int index)
        {
            return (T)Values[index];
        }

        public T GetValue<T>()
        {
            return GetValue<T>(0);
        }
    }
}
