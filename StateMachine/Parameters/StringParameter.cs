using System;

namespace StateMachineUtil.Parameters
{
    public class StringParameter : Parameter
    {
        #region Properties
        public new string Value
        {
            get
            {
                return (string)_value;
            }
        }
        #endregion

        #region Constructors
        public StringParameter(string name, object initialValue) : base(name, initialValue) { }
        #endregion

        #region Overloads
        public static bool operator ==(StringParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(string))
            {
                var right = (string)rhs;
                return lhs.Value == right;
            }            
            else if (rhs.GetType() == typeof(StringParameter))
            {
                var right = (StringParameter)rhs;
                return lhs.Value == right.Value;
            }            
            return false;
        }

        public static bool operator !=(StringParameter lhs, object rhs)
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
