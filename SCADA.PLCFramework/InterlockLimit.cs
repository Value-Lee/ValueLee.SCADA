using System;
using System.Collections.Generic;
using System.Text;

namespace SCADA.PLCFramework
{
    public class InterlockLimit
    {
        public InterlockLimit(string name, bool limitValue)
        {
            Name = name;
            LimitValue = limitValue;
            ByPass = false;
        }

        public bool ByPass { get; internal set; }
        public bool LimitValue { get; }
        public string Name { get; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj is InterlockLimit limit &&
                   Name == limit.Name &&
                   LimitValue == limit.LimitValue;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, LimitValue);
        }
        public static bool operator ==(InterlockLimit left, InterlockLimit right)
        {
            if (left is null && right is null)
                return true;
            if (left is null || right is null)
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(InterlockLimit left, InterlockLimit right)
        {
            return !(left == right);
        }
    }
}