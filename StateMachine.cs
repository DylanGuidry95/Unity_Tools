using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine; 
namespace DG_State_Machine
{   
    [System.Serializable]
    public class StateMachine
    {        
        [JsonProperty]
        private Dictionary<string, State> States;
        [JsonProperty]
        private Dictionary<string, Transition> Transitions;                
        
        private List<IntParameter> IntParameters;        
        private List<StringParameter> StringParameters;        
        private List<FloatParameter> FloatParameters;        
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

        public StateMachine()
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
            CurrentState.ExitState();
            CurrentState = state;            
            CurrentState.EnterState();
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
            if (Parameters.ContainsKey(name))
                return -1;
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

        public void AddParameters(List<StateMachineParameter> parameters)
        {
            foreach(var p in parameters)
            {
                if (Parameters.ContainsKey(p.Name))
                    continue;
                if(p.GetType() == typeof(BooleanParamater))
                {
                    BooleanParameters.Add(p as BooleanParamater);
                }
                if (p.GetType() == typeof(IntParameter))
                {
                    IntParameters.Add(p as IntParameter);
                }
                if (p.GetType() == typeof(StringParameter))
                {
                    StringParameters.Add(p as StringParameter);
                }
                if (p.GetType() == typeof(FloatParameter))
                {
                    FloatParameters.Add(p as FloatParameter);
                }
            }            
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

        public StateMachine BuildMachineFromFile(string data, bool isPath = true)
        {
            var content = "";
            if (isPath)
            {
                if (!System.IO.File.Exists(data))
                    return null;
                content = System.IO.File.ReadAllText(data);
                if (data == "")
                    return null;
            }
            else
                content = data;

            var builtMachine = new StateMachine();
            var lineSplit = content.Split('\n');

            for (int i = 0; i < lineSplit.Length; i++)
            {
                if (lineSplit[i][0] == '#')
                {
                    continue;
                }
                if (lineSplit[i] == "[Parameters]\r")
                {
                    if (lineSplit[i + 1] == "{\r")
                    {
                        var arg = "";
                        for (int j = i + 2; lineSplit[j] != "}\r"; j++)
                        {
                            lineSplit[j] = lineSplit[j].Trim();
                            arg += lineSplit[j];
                        }
                        builtMachine.AddParameters(CreateParameters(arg));
                    }
                }
                if (lineSplit[i].Contains("[State]\r"))
                {
                    if (lineSplit[i + 1] == "{\r")
                    {
                        for (int j = i + 2; lineSplit[j] != "}\r"; j++)
                        {                            
                            lineSplit[j] = lineSplit[j].Trim();                            
                            if(lineSplit[j].Contains(";"))
                                builtMachine.AddState(lineSplit[j].Remove(lineSplit[j].Length - 1));
                            else
                                builtMachine.AddState(lineSplit[j]);
                        }
                    }
                }
                if (lineSplit[i].Contains("[Transitions]\r"))
                {
                    if (lineSplit[i + 1] == "{\r")
                    {
                        for (int j = i + 2; lineSplit[j] != "}"; j++)
                        {
                            lineSplit[j] = lineSplit[j].Trim();
                            var states = lineSplit[j].Split('>');
                            if (states.Length <= 1)
                                break;
                            if(lineSplit[j].Contains(";"))
                                builtMachine.DefineTransition(builtMachine.States[states[0]], builtMachine.States[states[1].Remove(states[1].Length - 1)]);
                            else
                                builtMachine.DefineTransition(builtMachine.States[states[0]], builtMachine.States[states[1]]);
                        }
                    }
                }

                if(lineSplit[i].Contains("Condition = "))
                {
                    var getTransition = lineSplit[i].Split('=');                    
                    getTransition[1] = getTransition[1].Trim();
                    var states = getTransition[1].Split('>');
                    states[1] = states[1].Remove(states[1].Length - 1);
                    var transition = builtMachine.GetTransition(states[0], states[1]);
                    if (transition != null && lineSplit[i + 1] == "{\r")
                    {
                        for(int j = i + 2; lineSplit[j] != "}"; j++)
                        {
                            var line = lineSplit[j].Trim();
                            var con = line.Split(' ');
                            if(con.Length == 3)
                            {                                
                                switch(con[1])
                                {
                                    case ">":
                                        transition.DefineCondition(BuildCondition(con[0], Condition.opperation.greater_than, con[2]));
                                        break;
                                    case "<":
                                        transition.DefineCondition(BuildCondition(con[0], Condition.opperation.less_than, con[2]));
                                        break;
                                    case ">=":
                                        transition.DefineCondition(BuildCondition(con[0], Condition.opperation.greater_than_equals, con[2]));
                                        break;
                                    case "<=":
                                        transition.DefineCondition(BuildCondition(con[0], Condition.opperation.less_than_equals, con[2]));
                                        break;
                                    case "==":
                                        transition.DefineCondition(BuildCondition(con[0], Condition.opperation.equals, con[2]));
                                        break;
                                    case "!=":
                                        transition.DefineCondition(BuildCondition(con[0], Condition.opperation.not_equals, con[2]));
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return builtMachine;
        }        

        Condition BuildCondition(string lhs, Condition.opperation op, string rhs)
        {
            if (!Parameters.ContainsKey(lhs))
                return null;
            if (rhs.Contains(";"))
                rhs = rhs.Remove(rhs.Length - 1);
            rhs = rhs.Trim();
            Condition con = null;
            if (int.TryParse(rhs, out int intVal))
            {
                con = new Condition(Parameters[lhs], intVal, op);
            }
            else if (float.TryParse(rhs, out float floatVal))
            {
                con = new Condition(Parameters[lhs], floatVal, op);
            }
            else if (rhs == "true")
            {
                con = new Condition(Parameters[lhs], true, op);
            }
            else if (rhs == "false")
            {
                con = new Condition(Parameters[lhs], false, op);
            }
            else
                con = new Condition(Parameters[lhs], rhs, op);
            return con;
        }

        List<StateMachineParameter> CreateParameters(string parameters)
        {
            var parametersObj = new List<StateMachineParameter>();
            var parSplit = parameters.Split(';');
            foreach(var par in parSplit)
            {
                //Check for type
                var type = "";
                var name = "";
                bool gettingType = false;
                
                for(int i = 0; i < par.Length; i++)
                {
                    if (par[i] == '(' && !gettingType)
                    {
                        gettingType = true;                       
                    }
                    else if (par[i] == ')' && gettingType)
                    {
                        gettingType = false;                        
                    }
                    else if (gettingType)
                    {
                        type += par[i];                        
                    }
                    else
                        name += par[i];
                }
                name = name.Trim();
                switch(type)
                {
                    case "float":
                        var newFloat = new FloatParameter(name, 0.0f);
                        parametersObj.Add(newFloat);
                        break;
                    case "int":
                        var newInt = new IntParameter(name, 0);
                        parametersObj.Add(newInt);
                        break;
                    case "string":
                        var newString = new StringParameter(name, "");
                        parametersObj.Add(newString);
                        break;
                    case "bool":
                        var newBool = new BooleanParamater(name, true);
                        parametersObj.Add(newBool);
                        break;
                    default:
                        Debug.LogError(name + " does not have a valid type declared");
                        break;
                }
            }
            return parametersObj;
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
