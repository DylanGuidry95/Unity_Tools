using System;

namespace StateMachineUtil.Parameters
{
    public class IntParameter : Parameter
    {
        #region Properties
        public new int Value
        {
            get
            {
                return (int)_value;
            }
        }
        #endregion

        #region Constructors
        public IntParameter(string name, object initialValue) : base(name, initialValue) { }        
        #endregion

        #region Overloads
        public static bool operator ==(IntParameter lhs, object rhs)
        {
            if(rhs.GetType() == typeof(int))
            {
                var right = (int)rhs;
                return lhs.Value == right;
            }
            else if(rhs.GetType() == typeof(float))
            {
                var right = (float)rhs;
                return lhs.Value == right;
            }
            else if(rhs.GetType() == typeof(IntParameter))
            {
                var right = (IntParameter)rhs;
                return lhs.Value == right.Value;
            }
            else if (rhs.GetType() == typeof(FloatParameter))
            {
                var right = (FloatParameter)rhs;
                return lhs.Value == right.Value;
            }
            return false;
        }

        public static bool operator !=(IntParameter lhs, object rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator <(IntParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(int))
            {
                var right = (int)rhs;
                return lhs.Value < right;
            }
            else if (rhs.GetType() == typeof(float))
            {
                var right = (float)rhs;
                return lhs.Value < right;
            }
            else if (rhs.GetType() == typeof(IntParameter))
            {
                var right = (IntParameter)rhs;
                return lhs.Value < right.Value;
            }
            else if (rhs.GetType() == typeof(FloatParameter))
            {
                var right = (FloatParameter)rhs;
                return lhs.Value < right.Value;
            }
            return false;
        }

        public static bool operator >(IntParameter lhs, object rhs)
        {            
            return !(lhs < rhs);
        }

        public static bool operator <=(IntParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(int))
            {
                var right = (int)rhs;
                return lhs.Value <= right;
            }
            else if (rhs.GetType() == typeof(float))
            {
                var right = (float)rhs;
                return lhs.Value <= right;
            }
            else if (rhs.GetType() == typeof(IntParameter))
            {
                var right = (IntParameter)rhs;
                return lhs.Value <= right.Value;
            }
            else if (rhs.GetType() == typeof(FloatParameter))
            {
                var right = (FloatParameter)rhs;
                return lhs.Value <= right.Value;
            }
            return false;
        }

        public static bool operator >=(IntParameter lhs, object rhs)
        {
            return !(lhs <= rhs);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
