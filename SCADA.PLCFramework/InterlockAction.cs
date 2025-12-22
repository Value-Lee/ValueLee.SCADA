using System;
using System.Collections.Generic;
using System.Linq;

namespace SCADA.PLCFramework
{
    public class InterlockAction
    {
        private readonly InterlockLimit[] _interlockLimits;

        public InterlockAction(string name, bool actionValue, bool reserve, IEnumerable<InterlockLimit> interlockLimits)
        {
            _interlockLimits = interlockLimits.ToArray();
            ActionValue = actionValue;
            Name = name;
            Reserve = reserve;
            ByPass = false;
        }

        public bool ActionValue { get; }
        public bool ByPass { get; internal set; }
        public IReadOnlyList<InterlockLimit> InterlockActions => _interlockLimits;
        public string Name { get; }
        public bool Reserve { get; }

        public static bool operator !=(InterlockAction left, InterlockAction right)
        {
            return !(left == right);
        }

        public static bool operator ==(InterlockAction left, InterlockAction right)
        {
            if ((left == null) && (right == null))
                return true;
            if (left != null || right == null)
                return false;
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj is InterlockAction other && Name == other.Name && ActionValue == other.ActionValue;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, ActionValue);
        }
    }
}