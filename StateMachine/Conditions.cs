using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineUtil
{
    public class Condition
    {
        #region Fields
        private StateMachineUtil.Parameters.Parameter _leftHandSide;
        private object _rightHandSide;
        private ConditionOperations _operation;
        #endregion

        #region Properties
        #endregion

        #region Constructor
        public Condition(StateMachineUtil.Parameters.Parameter lhs, ConditionOperations operation, object rhs)
        {
            _leftHandSide = lhs;
            _operation = operation;
            _rightHandSide = rhs;
        }
        #endregion

        #region Methods
        public bool Evaluate()
        {
            switch(_operation)
            {
                case ConditionOperations.equals_to:
                    if (_leftHandSide is Parameters.IntParameter)
                        return (_leftHandSide as Parameters.IntParameter) == _rightHandSide;
                    else if (_leftHandSide is Parameters.FloatParameter)
                        return (_leftHandSide as Parameters.FloatParameter) == _rightHandSide;
                    else if (_leftHandSide is Parameters.StringParameter)
                        return (_leftHandSide as Parameters.StringParameter) == _rightHandSide;
                    else if (_leftHandSide is Parameters.BooleanParameter)
                        return (_leftHandSide as Parameters.BooleanParameter) == _rightHandSide;
                    else
                        break;
                case ConditionOperations.not_equals_to:
                    if (_leftHandSide is Parameters.IntParameter)
                        return (_leftHandSide as Parameters.IntParameter) != _rightHandSide;
                    else if (_leftHandSide is Parameters.FloatParameter)
                        return (_leftHandSide as Parameters.FloatParameter) != _rightHandSide;
                    else if (_leftHandSide is Parameters.StringParameter)
                        return (_leftHandSide as Parameters.StringParameter) != _rightHandSide;
                    else if (_leftHandSide is Parameters.BooleanParameter)
                        return (_leftHandSide as Parameters.BooleanParameter) != _rightHandSide;
                    else
                        break;
                case ConditionOperations.less_than:
                    if (_leftHandSide is Parameters.IntParameter)
                        return (_leftHandSide as Parameters.IntParameter) < _rightHandSide;
                    else if (_leftHandSide is Parameters.FloatParameter)
                        return (_leftHandSide as Parameters.FloatParameter) < _rightHandSide;
                    else
                        break;
                case ConditionOperations.less_than_or_equals_to:
                    if (_leftHandSide is Parameters.IntParameter)
                        return (_leftHandSide as Parameters.IntParameter) <= _rightHandSide;
                    else if (_leftHandSide is Parameters.FloatParameter)
                        return (_leftHandSide as Parameters.FloatParameter) <= _rightHandSide;
                    else
                        break;
                case ConditionOperations.greater_than:
                    if (_leftHandSide is Parameters.IntParameter)
                        return (_leftHandSide as Parameters.IntParameter) > _rightHandSide;
                    else if (_leftHandSide is Parameters.FloatParameter)
                        return (_leftHandSide as Parameters.FloatParameter) > _rightHandSide;
                    else
                        break;
                case ConditionOperations.greater_than_or_equals_to:
                    if (_leftHandSide is Parameters.IntParameter)
                        return (_leftHandSide as Parameters.IntParameter) > _rightHandSide;
                    else if (_leftHandSide is Parameters.FloatParameter)
                        return (_leftHandSide as Parameters.FloatParameter) >= _rightHandSide;
                    else
                        break;
            }
            return false;
        }
        #endregion
    }
}
