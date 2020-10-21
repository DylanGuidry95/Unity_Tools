using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine; 
namespace Visual_Test
{
    [JsonObject(MemberSerialization.OptIn), Serializable]
    public class StateMachine
    {        
        [JsonProperty]
        private Dictionary<string, State> States;
        [JsonProperty]
        private Dictionary<string, Transition> Transitions;
        [JsonProperty, SerializeField]
        private List<StateMachineParameter> MachineParameters;

        [JsonProperty]
        public Dictionary<string, StateMachineParameter> Parameters 
        {
            get
            {
                var temp = new Dictionary<string, StateMachineParameter>();
                if(MachineParameters != null)
                    foreach(var p in MachineParameters)
                    {
                        temp.Add(p.Name, p);
                    }
                return temp;
            }
        }

        public StateMachine()
        {
            MachineParameters = new List<StateMachineParameter>();
            States = new Dictionary<string, State>();
            Transitions = new Dictionary<string, Transition>();
        }

        public StateMachine(object[] states) : this()
        {                        
            foreach(var s in states)
            {
                if(s.GetType() == typeof(string))
                {
                    AddState((string)s);
                }
                if(s.GetType() == typeof(State))
                {
                    AddState(((State)s));
                }
            }
        }

        public StateMachine(object[] states, object[] transitions) : this(states)
        {
            foreach(var t in transitions)
            {
                if(t.GetType() == typeof(object[]))
                {
                    var obj_arr = (object[])t;
                    State from = null, to = null;
                    if (obj_arr[0].GetType() == typeof(string))
                        from = GetState((string)obj_arr[0]);
                    else if (obj_arr[0].GetType() == typeof(State))
                        from = (State)obj_arr[0];
                    if(obj_arr[1].GetType() == typeof(string))
                        to = GetState((string)obj_arr[1]);
                    else if (obj_arr[1].GetType() == typeof(State))
                        to = (State)obj_arr[1];

                    DefineTransition(from, to);
                }
                else if(t.GetType() == typeof(Transition))
                {
                    if (!Transitions.ContainsKey(((Transition)t).Name))
                    {
                        Transitions.Add(((Transition)t).Name, (Transition)t);
                    }
                }
            }
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
            if (MachineParameters == null)
                MachineParameters = new List<StateMachineParameter>();
            foreach(var p in MachineParameters)
            {
                if (p.Name == name)
                    return -1;
            }
            if(value.GetType() == typeof(int))
            {
                MachineParameters.Add(new IntParameter(name, (int)value));
                return 0;
            }
            if (value.GetType() == typeof(float))
            {
                MachineParameters.Add(new FloatParameter(name, (float)value));
                return 0;
            }
            if (value.GetType() == typeof(string))
            {
                MachineParameters.Add(new StringParameter(name, (string)value));
                return 0;
            }
            if (value.GetType() == typeof(bool))
            {
                MachineParameters.Add(new BooleanParamater(name, (bool)value));
                return 0;
            }
            return -2;
        }        

        public void RemoveParamter(string name)
        {
            if (Parameters.ContainsKey(name))
            {
                MachineParameters.Remove(Parameters[name]);
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
