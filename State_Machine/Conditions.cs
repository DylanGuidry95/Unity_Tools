using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json;

namespace Visual_Test
{
    public class Condition
    {
        [JsonProperty]
        public StateMachineParameter LHS;
        [JsonProperty]
        public object RHS;
 
        private Type type { get { return LHS.GetType(); } }

        public enum opperation
        {
            equals,
            not_equals,
            less_than,
            greater_than,
            less_than_equals,
            greater_than_equals
        }

        [JsonProperty]
        private opperation Opperation;        

        public Condition(StateMachineParameter lhs, object rhs, opperation op)
        {
            Opperation = op;
            LHS = lhs;
            RHS = rhs;
        }

        public bool Evaulate()
        {
            switch(Opperation)
            {
                case opperation.equals:
                    if(LHS.GetType() == typeof(IntParameter))
                        return (IntParameter)LHS == RHS;
                    if (LHS.GetType() == typeof(StringParameter))
                        return (StringParameter)LHS == RHS;
                    if (LHS.GetType() == typeof(FloatParameter))
                        return (FloatParameter)LHS == RHS;
                    break;
                case opperation.not_equals:
                    if (LHS.GetType() == typeof(IntParameter))
                        return (IntParameter)LHS != RHS;
                    if (LHS.GetType() == typeof(StringParameter))
                        return (StringParameter)LHS != RHS;
                    if (LHS.GetType() == typeof(FloatParameter))
                        return (FloatParameter)LHS != RHS;
                    break;
                case opperation.less_than:
                    if (LHS.GetType() == typeof(IntParameter))
                        return (IntParameter)LHS < RHS;
                    if (LHS.GetType() == typeof(StringParameter))
                        return (StringParameter)LHS < RHS;
                    if (LHS.GetType() == typeof(FloatParameter))
                        return (FloatParameter)LHS < RHS;
                    break;
                case opperation.greater_than:
                    if (LHS.GetType() == typeof(IntParameter))
                        return (IntParameter)LHS > RHS;
                    if (LHS.GetType() == typeof(StringParameter))
                        return (StringParameter)LHS > RHS;
                    if (LHS.GetType() == typeof(FloatParameter))
                        return (FloatParameter)LHS > RHS;
                    break;
                case opperation.less_than_equals:
                    if (LHS.GetType() == typeof(IntParameter))
                        return (IntParameter)LHS <= RHS;
                    if (LHS.GetType() == typeof(StringParameter))
                        return (StringParameter)LHS <= RHS;
                    if (LHS.GetType() == typeof(FloatParameter))
                        return (FloatParameter)LHS <= RHS;
                    break;
                case opperation.greater_than_equals:
                    if (LHS.GetType() == typeof(IntParameter))
                        return (IntParameter)LHS >= RHS;
                    if (LHS.GetType() == typeof(StringParameter))
                        return (StringParameter)LHS >= RHS;
                    if (LHS.GetType() == typeof(FloatParameter))
                        return (FloatParameter)LHS >= RHS;
                    break;
            }

            return false;
        }        

        public static bool operator == (Condition lhs, Condition rhs)
        {
            return lhs.LHS == rhs.LHS && lhs.RHS == rhs.RHS && lhs.Opperation == rhs.Opperation;
        }

        public static bool operator !=(Condition lhs, Condition rhs)
        {
            return lhs.LHS != rhs.LHS || lhs.RHS != rhs.RHS || lhs.Opperation != rhs.Opperation;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Condition))
                return false;
            var rhs = (Condition)obj;
            return this.LHS == rhs.LHS && this.RHS == rhs.RHS && this.Opperation == rhs.Opperation;
        }
    }

    [Serializable]
    public class StateMachineParameter
    {
        public string Name;
                        
        public object Stored_Value;
        
        public enum ParmaterType
        {
            _int, _float, _string, _bool
        }

        public object Value { get { return Stored_Value; } }

        public StateMachineParameter(string name, object value)
        {
            Name = name;
            Stored_Value = value;
        }        

        public void SetValue(object value) 
        {
            Stored_Value = value;
        }

        public static bool operator ==(StateMachineParameter lhs, object rhs)
        {
            return lhs == rhs;            
        }

        public static bool operator !=(StateMachineParameter lhs, object rhs)
        {
            return lhs == rhs;
        }

        public static bool operator <(StateMachineParameter lhs, object rhs)
        {

#pragma warning disable CS1030 // #warning directive
#warning Inequality operation not defined with this type will always return FALSE
            return false;
#pragma warning restore CS1030 // #warning directive
        }

        public static bool operator >(StateMachineParameter lhs, object rhs)
        {

#pragma warning disable CS1030 // #warning directive
#warning Inequality operation not defined with this type will always return FALSE
            return false;
#pragma warning restore CS1030 // #warning directive
        }

        public static bool operator <=(StateMachineParameter lhs, object rhs)
        {

#pragma warning disable CS1030 // #warning directive
#warning Inequality operation not defined with this type will always return FALSE
            return false;
#pragma warning restore CS1030 // #warning directive
        }

        public static bool operator >=(StateMachineParameter lhs, object rhs)
        {

#pragma warning disable CS1030 // #warning directive
#warning Inequality operation not defined with this type will always return FALSE
            return false;
#pragma warning restore CS1030 // #warning directive
        }
    }

    [Serializable]
    public class StringParameter : StateMachineParameter
    {        
        public string InternalValue;
        public string stored_Value { get { InternalValue = (string)Stored_Value; return InternalValue; } }

        public StringParameter(string name, string value) : base(name, value) { }

        public new void SetValue(object value)
        {            
            Stored_Value = InternalValue = (string)value;
        }

        public static bool operator ==(StringParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(string))
                return lhs.stored_Value == (string)rhs;
            else if (rhs.GetType() == typeof(StringParameter))
                return lhs.stored_Value == ((StringParameter)(rhs)).stored_Value;
            return false;
        }

        public static bool operator !=(StringParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(string))
                return lhs.stored_Value != (string)rhs;
            else if (rhs.GetType() == typeof(StringParameter))
                return lhs.Stored_Value != ((StringParameter)(rhs)).Stored_Value;
            return false;
        }

    }

    [Serializable]
    public class BooleanParamater : StateMachineParameter
    {        
        public bool InternalValue;
        public bool stored_Value { get { InternalValue = (bool)Stored_Value; return (bool)Stored_Value; } }

        public BooleanParamater(string name, bool value) : base(name, value) { }

        public new void SetValue(object value)
        {
            Stored_Value = InternalValue = (bool)value;
        }

        public static bool operator ==(BooleanParamater lhs, object rhs)
        {
            if (rhs.GetType() == typeof(bool))
                return lhs.stored_Value == (bool)rhs;
            else if (rhs.GetType() == typeof(BooleanParamater))
                return lhs.stored_Value == ((BooleanParamater)(rhs)).stored_Value;
            return false;
        }

        public static bool operator !=(BooleanParamater lhs, object rhs)
        {
            if (rhs.GetType() == typeof(bool))
                return lhs.stored_Value != (bool)rhs;
            else if (rhs.GetType() == typeof(BooleanParamater))
                return lhs.Stored_Value != ((BooleanParamater)(rhs)).Stored_Value;
            return false;
        }
    }

    [Serializable]
    public class IntParameter : StateMachineParameter
    {        
        public int InternalValue;
        public int stored_Value { get { InternalValue = (int)Stored_Value; return (int)Stored_Value; } }

        public IntParameter(string name, int value) : base(name, value) { }

        public new void SetValue(object value)
        {
            Stored_Value = InternalValue = (int)value;
        }

        public static bool operator ==(IntParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(int))
                return lhs.stored_Value == (int)rhs;
            else if (rhs.GetType() == typeof(IntParameter))
                return lhs.stored_Value == ((IntParameter)(rhs)).stored_Value;
            return false;
        }

        public static bool operator !=(IntParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(int))
                return lhs.stored_Value != (int)rhs;
            else if (rhs.GetType() == typeof(IntParameter))
                return lhs.stored_Value != ((IntParameter)(rhs)).stored_Value;
            return false;
        }

        public static bool operator <(IntParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(int))
                return lhs.stored_Value < (int)rhs;
            else if (rhs.GetType() == typeof(IntParameter))
                return lhs.stored_Value < ((IntParameter)(rhs)).stored_Value;
            return false;
        }

        public static bool operator >(IntParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(int))
                return lhs.stored_Value > (int)rhs;
            else if (rhs.GetType() == typeof(IntParameter))
                return lhs.stored_Value > ((IntParameter)(rhs)).stored_Value;
            return false;
        }

        public static bool operator >=(IntParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(int))
                return lhs.stored_Value >= (int)rhs;
            else if (rhs.GetType() == typeof(IntParameter))
                return lhs.stored_Value >= ((IntParameter)(rhs)).stored_Value;
            return false;
        }

        public static bool operator <=(IntParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(int))
                return lhs.stored_Value <= (int)rhs;
            else if (rhs.GetType() == typeof(IntParameter))
                return lhs.stored_Value <= ((IntParameter)(rhs)).stored_Value;
            return false;
        }
    }

    [Serializable]
    public class FloatParameter : StateMachineParameter
    {        
        public float InternalValue;
        public float stored_Value { get { InternalValue = (float)Stored_Value; return (float)Stored_Value; } }

        public FloatParameter(string name, float value) : base(name, value) { }

        public new void SetValue(object value)
        {
            Stored_Value = InternalValue = (float)value;
        }

        public static bool operator ==(FloatParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(float))
                return lhs.stored_Value == (float)rhs;
            else if (rhs.GetType() == typeof(FloatParameter))
                return lhs.stored_Value == ((FloatParameter)(rhs)).stored_Value;
            return false;
        }

        public static bool operator !=(FloatParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(float))
                return lhs.stored_Value != (float)rhs;
            else if (rhs.GetType() == typeof(FloatParameter))
                return lhs.stored_Value != ((FloatParameter)(rhs)).stored_Value;
            return false;
        }

        public static bool operator <(FloatParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(float))
                return lhs.stored_Value < (float)rhs;
            else if (rhs.GetType() == typeof(FloatParameter))
                return lhs.stored_Value < ((FloatParameter)(rhs)).stored_Value;
            return false;
        }

        public static bool operator >(FloatParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(float))
                return lhs.stored_Value > (float)rhs;
            else if (rhs.GetType() == typeof(FloatParameter))
                return lhs.stored_Value > ((FloatParameter)(rhs)).stored_Value;
            return false;
        }

        public static bool operator >=(FloatParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(float))
                return lhs.stored_Value >= (float)rhs;
            else if (rhs.GetType() == typeof(FloatParameter))
                return lhs.stored_Value >= ((FloatParameter)(rhs)).stored_Value;
            return false;
        }

        public static bool operator <=(FloatParameter lhs, object rhs)
        {
            if (rhs.GetType() == typeof(float))
                return lhs.stored_Value <= (float)rhs;
            else if (rhs.GetType() == typeof(FloatParameter))
                return lhs.stored_Value <= ((FloatParameter)(rhs)).stored_Value;
            return false;
        }

        public static implicit operator float(FloatParameter param) => param.stored_Value;
    }
}
