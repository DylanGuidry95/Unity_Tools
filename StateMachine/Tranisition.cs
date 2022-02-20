using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineUtil
{
    public class Tranisition
    {
        #region Fields
        private State _startState;
        private State _endState;
        private List<Condition> _conditions;
        #endregion

        #region Properties
        public string StartState
        {
            get
            {
                return _startState.Name;
            }
        }

        public string EndState
        {
            get
            {
                return _endState.Name;
            }
        }
        #endregion

        #region Constructors
        public Tranisition(State start, State end, List<Condition> conditions = null)
        {
            _startState = start;
            _endState = end;
            _conditions = conditions;
        }        
        #endregion

        #region Methods
        public bool CanTransition()
        {
            foreach(var condition in _conditions)
            {
                if (!condition.Evaluate())
                    return false;
            }
            return true;
        }

        public void DefineCondition(Condition condition)
        {
            if (_conditions == null)
                _conditions = new List<Condition>();
            _conditions.Add(condition);
        }

        public void DefineCondition(StateMachineUtil.Parameters.Parameter lhs, ConditionOperations operation, object rhs)
        {
            _conditions.Add(new Condition(lhs, operation, rhs));
        }
        #endregion

        #region Overloads
        #endregion
    }
}
