using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineUtil
{
    public class StateMachine<T>
    {
        #region Fields
        private List<State<T>> _states;
        private List<Tranisition<T>> _transitions;
        private List<StateMachineUtil.Parameters.Parameter> _parameters;
        private State<T> _currentState;

        public bool AllowAutoTransition;
        #endregion

        #region Properties
        public T CurrentState
        {
            get
            {
                return _currentState.Name;
            }
        }

        public Dictionary<State<T>, List<Tranisition<T>>> TransitionFromState
        {
            get
            {
                var returnDictionary = new Dictionary<State<T>, List<Tranisition<T>>>();
                foreach (var state in _states)
                {
                    var transitions = new List<Tranisition<T>>();
                    foreach (var transition in _transitions)
                    {
                        if (transition.StartState.ToString() == state.Name.ToString())
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
            _states = new List<State<T>>();
            _transitions = new List<Tranisition<T>>();
            _parameters = new List<Parameters.Parameter>();
            AllowAutoTransition = autoTransition;
        }

        public StateMachine(List<State<T>> states, List<Tranisition<T>> tranisitions, List<StateMachineUtil.Parameters.Parameter> parameters = null, bool autoTransition = false)
        {
            _states = states;
            _transitions = tranisitions;
            _parameters = parameters;
            AllowAutoTransition = autoTransition;
        }
        #endregion

        #region Methods
        public void StartMachine(T startState)
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

        public State<T> AddState(T stateName)
        {
            var newState = new State<T>(stateName);
            _states.Add(newState);
            return newState;
        }

        public Parameters.Parameter AddParameter(Parameters.Parameter parameter)
        {
            _parameters.Add(parameter);
            return parameter;
        }

        public Tranisition<T> AddTransition(T startState, T endState, List<Condition> conditions = null)
        {
            var newTransition = new Tranisition<T>(GetStateByName(startState), GetStateByName(endState));
            _transitions.Add(newTransition);
            return newTransition;
        }

        private void GoToState(T stateName)
        {
            _currentState.Exit();
            _currentState = GetStateByName(stateName);
            _currentState.Enter();
        }

        private State<T> GetStateByName(T name)
        {
            foreach(var state in _states)
            {
                if (state.Name.ToString() == name.ToString())
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

        private List<Tranisition<T>> GetAllTransitionsFromState(T stateName)
        {
            var retList = new List<Tranisition<T>>();
            foreach (var transition in _transitions)
            {
                if (transition.StartState.ToString() == stateName.ToString())
                {
                    retList.Add(transition);
                }
            }
            return retList;
        }

        private List<Tranisition<T>> GetAllTransitionsFromState(State<T> state)
        {
            return GetAllTransitionsFromState(state.Name);
        }
        #endregion
    }
}
