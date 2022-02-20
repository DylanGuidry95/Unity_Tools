using System;

namespace StateMachineUtil.Parameters
{
    public class Parameter
    {
        #region Fields
        protected string _name;
        protected object _value;        
        #endregion

        #region Properties
        public object Value
        {
            set
            {
                if (value.GetType() == _value.GetType())
                    _value = value;
            }
            get
            {
                return _value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }
        #endregion

        #region Constructors
        public Parameter(string name, object initialValue)
        {
            _name = name;
            _value = initialValue;
        }
        #endregion

        #region Overloads
        public static bool operator==(Parameter lhs, object rhs)
        {
            return false;
        }

        public static bool operator !=(Parameter lhs, object rhs)
        {
            return false;
        }

        public static bool operator <(Parameter lhs, object rhs)
        {
            return false;
        }

        public static bool operator >(Parameter lhs, object rhs)
        {
            Console.WriteLine("Parent Hit");
            return false;
        }

        public static bool operator <=(Parameter lhs, object rhs)
        {
            return false;
        }

        public static bool operator >=(Parameter lhs, object rhs)
        {
            return false;
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
