using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Visual_Test
{
    [JsonObject(MemberSerialization.OptIn)]
    class Transition
    {
        [JsonProperty]
        private State Current;
        [JsonProperty]
        private State Destination;
        
        public string From { get { return Current.Name; } }
        public string To { get { return Destination.Name; } }

        public string Name { get { return Current.Name + ">" + Destination.Name; } }

        [JsonProperty]
        private List<Condition> Conditions;

        public bool CanTransition;        
        
        public Transition()
        { 
            Conditions = new List<Condition>(); 
        }

        public Transition(State c, State d) : this()
        {
            Current = c;
            Destination = d;
        }

        public void Update()
        {
            foreach(var c in Conditions)
            {
                if (c.Evaulate() == false)
                {
                    CanTransition = false;
                    return;
                }
            }
            CanTransition = true;
            return;
        }

        public int DefineCondition(StateMachineParameter lhs, object rhs, Condition.opperation opp)
        {
            var new_condition = new Condition(lhs, rhs, opp);
            if (Conditions.Contains(new_condition))
                return -1;
            Conditions.Add(new_condition);
            return 0;
        }
    }
}
