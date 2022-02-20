using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineUtil
{
    public class StateMachine
    {
        #region Fields
        private List<State> _states;
        private List<Tranisition> _transitions;
        private List<StateMachineUtil.Parameters.Parameter> _parameters;
        private State _currentState;

        public bool AllowAutoTransition;
        #endregion

        #region Properties
        public string CurrentState
        {
            get
            {
                return _currentState.Name;
            }
        }

        public Dictionary<State, List<Tranisition>> TransitionFromState
        {
            get
            {
                var returnDictionary = new Dictionary<State, List<Tranisition>>();
                foreach (var state in _states)
                {
                    var transitions = new List<Tranisition>();
                    foreach (var transition in _transitions)
                    {
                        if (transition.StartState == state.Name)
                        {
                            transitions.Add(transition);
                        }
                    }
                    returnDictionary.Add(state, transitions);
                }
                return returnDictionary;
            }
        }        
        #endregion

        #region Constructors
        public StateMachine(bool autoTransition = false)
        {
            _states = new List<State>();
            _transitions = new List<Tranisition>();
            _parameters = new List<Parameters.Parameter>();
            AllowAutoTransition = autoTransition;
        }

        public StateMachine(List<State> states, List<Tranisition> tranisitions, List<StateMachineUtil.Parameters.Parameter> parameters = null, bool autoTransition = false)
        {
            _states = states;
            _transitions = tranisitions;
            _parameters = parameters;
            AllowAutoTransition = autoTransition;
        }
        #endregion

        #region Methods
        public void StartMachine(string startState)
        {
            _currentState = GetStateByName(startState);
            _currentState.Enter();
        }
        public void Update()
        {
            _currentState.Update();
            if (!AllowAutoTransition)
                return;
            foreach(var transition in GetAllTransitionsFromState(_currentState))
            {
                if (transition.CanTransition())
                {
                    GoToState(transition.EndState);
                    break;
                }
            }
        }

        public void SetParameter(string name, object value)
        {
            GetParameterByName(name).Value = value;
        }

        public State AddState(string stateName)
        {
            var newState = new State(stateName);
            _states.Add(newState);
            return newState;
        }

        public Parameters.Parameter AddParameter(Parameters.Parameter parameter)
        {
            _parameters.Add(parameter);
            return parameter;
        }

        public Tranisition AddTransition(string startState, string endState, List<Condition> conditions = null)
        {
            var newTransition = new Tranisition(GetStateByName(startState), GetStateByName(endState));
            _transitions.Add(newTransition);
            return newTransition;
        }

        private void GoToState(string stateName)
        {
            _currentState.Exit();
            _currentState = GetStateByName(stateName);
            _currentState.Enter();
        }

        private State GetStateByName(string name)
        {
            foreach(var state in _states)
            {
                if (state.Name == name)
                    return state;
            }
            return null;
        }

        private Parameters.Parameter GetParameterByName(string name)
        {
            foreach(var parameter in _parameters)
            {
                if (parameter.Name == name)
                    return parameter;
            }
            return null;
        }

        private List<Tranisition> GetAllTransitionsFromState(string stateName)
        {
            var retList = new List<Tranisition>();
            foreach (var transition in _transitions)
            {
                if (transition.StartState == stateName)
                {
                    retList.Add(transition);
                }
            }
            return retList;
        }

        private List<Tranisition> GetAllTransitionsFromState(State state)
        {
            return GetAllTransitionsFromState(state.Name);
        }
        #endregion
    }
}
