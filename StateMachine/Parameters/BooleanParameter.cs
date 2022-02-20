using System;

namespace StateMachineUtil.Parameters
{
    public class BooleanParameter : Parameter
    {
        #region Properties
        public new bool Value
        {
            get
            {
                return (bool)_value;
            }
        }
        #endregion

        #region Constructors
        public BooleanParameter(string name, object initialValue) : base(name, initialValue) { }
        #endregion

        #region Overloads
        public static bool operator ==(BooleanParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(bool))
            {
                var right = (bool)rhs;
                return lhs.Value == right;
            }
            else if (rhs.GetType() == typeof(BooleanParameter))
            {
                var right = (BooleanParameter)rhs;
                return lhs.Value == right.Value;
            }
            return false;
        }

        public static bool operator !=(BooleanParameter lhs, object rhs)
        {
            return !(lhs == rhs);
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
