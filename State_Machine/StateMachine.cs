using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine; 
namespace Visual_Test
{
    [JsonObject(MemberSerialization.OptIn), Serializable, CreateAssetMenu(fileName = "NewStateMachine", menuName = "StateMachine")]
    public class StateMachine : ScriptableObject
    {        
        [JsonProperty]
        private Dictionary<string, State> States;
        [JsonProperty]
        private Dictionary<string, Transition> Transitions;                

        [SerializeField]
        private List<IntParameter> IntParameters;
        [SerializeField]
        private List<StringParameter> StringParameters;
        [SerializeField]
        private List<FloatParameter> FloatParameters;
        [SerializeField]
        private List<BooleanParamater> BooleanParameters;

        [JsonProperty]
        public Dictionary<string, StateMachineParameter> Parameters 
        {
            get
            {
                var temp = new Dictionary<string, StateMachineParameter>();
                if(IntParameters != null)
                    foreach(var i in IntParameters)
                    {
                        temp.Add(i.Name, i);
                    }
                if (StringParameters != null)
                    foreach (var i in StringParameters)
                    {
                        temp.Add(i.Name, i);
                    }
                if (FloatParameters != null)
                    foreach (var i in FloatParameters)
                    {
                        temp.Add(i.Name, i);
                    }
                if (BooleanParameters != null)
                    foreach (var i in BooleanParameters)
                    {
                        temp.Add(i.Name, i);
                    }
                return temp;
            }
        }

        public void CreateInstance()
        {
            IntParameters = new List<IntParameter>();
            FloatParameters = new List<FloatParameter>();
            StringParameters = new List<StringParameter>();
            BooleanParameters = new List<BooleanParamater>();
            States = new Dictionary<string, State>();
            Transitions = new Dictionary<string, Transition>();
        }
        
        public List<State> ExposeStates
        {
            get
            {
                var ret = new List<State>();
                if(States != null)
                    foreach (var s in States)
                    {
                        ret.Add(s.Value);
                    }
                return ret;
            }
        }
        
        public List<Transition> ExposeTransitions
        {
            get
            {
                var ret = new List<Transition>();
                if(Transitions != null)
                    foreach (var t in Transitions)
                    {
                        ret.Add(t.Value);
                    }
                return ret;
            }
        }
        
        [JsonProperty]
        public State CurrentState;

        public void Update()
        {
            foreach(var t in Transitions)
            {
                t.Value.Update();
                if(GetState(t.Value.From) == CurrentState)
                {
                    if (t.Value.CanTransition)
                        Transition(GetState(t.Value.To));
                }
            }
        }

        private void Transition(State state)
        {            
            CurrentState.ExitState(null);
            CurrentState = state;            
            CurrentState.EnterState(null);
        }

        public int AddState(object state)
        {
            if (States == null)
                States = new Dictionary<string, State>();
            State new_state = null;
            if (state.GetType() == typeof(string))
            {
                new_state = new State() { Name = (string)state };
            }
            if (state.GetType() == typeof(State))
            {
                if (States.ContainsKey(((State)state).Name))
                    return -2;
                new_state = (State)state;                
            }
            if (new_state == null)
                return -3;
            else
            {
                if (States.ContainsKey(new_state.Name))
                    return -4;
                States.Add(new_state.Name, new_state);
            }
            return -4;
        }

        public int RemoveState(object state)
        {
            if (States == null)
                return -1;

            if (state.GetType() == typeof(string))
                States.Remove((string)state);
            else if (state.GetType() == typeof(State))
                States.Remove(((State)state).Name);
            return 0;
        }


        public int DefineTransition(State from, State to)
        {
            if (States == null || from == null || to == null)
                return -1;            
            if (Transitions == null)
                Transitions = new Dictionary<string, Transition>();
            if (!States.ContainsKey(from.Name) || !States.ContainsKey(to.Name))
                return -2; //One of the transitions states are not defined in the state machine
            var t_name = from.Name + ">" + to.Name;
            if (Transitions.ContainsKey(t_name))
                return -3; //Transition has all ready been defined

            Transitions.Add(t_name, new Transition(from, to));
            return 0; //Transition has been defined and added to the state machine
        }

        public State GetState(string state_name)
        {
            if(States.ContainsKey(state_name))
                return States[state_name];
            return null;
        }

        public int AddParameter(string name, object value)
        {            
            foreach(var p in Parameters)
            {
                if (p.Key == name)
                    return -1;
            }
            if(value.GetType() == typeof(int))
            {
                IntParameters.Add(new IntParameter(name, (int)value));
                return 0;
            }
            if (value.GetType() == typeof(float))
            {
                FloatParameters.Add(new FloatParameter(name, (float)value));
                return 0;
            }
            if (value.GetType() == typeof(string))
            {
                StringParameters.Add(new StringParameter(name, (string)value));
                return 0;
            }
            if (value.GetType() == typeof(bool))
            {
                BooleanParameters.Add(new BooleanParamater(name, (bool)value));
                return 0;
            }
            return -2;
        }        

        public void RemoveParamter(string name)
        {
            if (Parameters.ContainsKey(name))
            {
                var removeItem = Parameters[name];
                if (removeItem.GetType() == typeof(IntParameter))
                    IntParameters.Remove((IntParameter)removeItem);
                if (removeItem.GetType() == typeof(StringParameter))
                    StringParameters.Remove((StringParameter)removeItem);
                if (removeItem.GetType() == typeof(FloatParameter))
                    FloatParameters.Remove((FloatParameter)removeItem);
                if (removeItem.GetType() == typeof(BooleanParamater))
                    BooleanParameters.Remove((BooleanParamater)removeItem);
            }
        }

        public Transition GetTransition(string from, string to)
        {
            if (Transitions.ContainsKey(from + ">" + to))
                return Transitions[from + ">" + to];
            else
                return null;
        }

        public List<Transition>GetAllTransitionsFrom(string origin)
        {
            var trans = new List<Transition>();
            foreach(var t in Transitions)
            {
                if (t.Value.From == origin)
                    trans.Add(t.Value);
            }
            return trans;
        }

        public List<Transition>GetAllTransitionsTo(string destination)
        {
            var trans = new List<Transition>();
            foreach(var t in Transitions)
            {
                if (t.Value.To == destination)
                    trans.Add(t.Value);
            }
            return trans;
        }

        public static StateMachine LoadMachine(string path)
        {
            StateMachine loaded_Machine = null;
            if (!System.IO.File.Exists(path))
                return null;
            var data = System.IO.File.ReadAllText(path);
            if (data == "")
                return null;
            loaded_Machine  = JsonConvert.DeserializeObject<StateMachine>(data);
            foreach(var state in loaded_Machine.States)
            {
                state.Value.BindDelegates();
            }
            return loaded_Machine;
        }        
    }
}
