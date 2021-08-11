using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DG_State_Machine
{
    [System.Serializable]
    public class State
    {
        public string Name;

        public delegate void OnStateEnter();
        public delegate void OnStateUpdate();
        public delegate void OnStateExit();

        [Newtonsoft.Json.JsonIgnore]
        public OnStateEnter StateEntered;
        [Newtonsoft.Json.JsonIgnore]
        public OnStateUpdate StateUpdate;
        [Newtonsoft.Json.JsonIgnore]
        public OnStateExit StateExited;

        public class DelegateData
        {
            public string DelegateName;
            public Type MethodHouse;
            public string MethodName;
        }

        public Dictionary<string,DelegateData> SerializeData;

        public State()
        {
            SerializeData = new Dictionary<string, DelegateData>();
        }

        public void AddEnterStateMethod(Type house, string methodName)
        {
            if (house == null || house.GetMethod(methodName) == null)
                return;
            if(StateEntered == null)
                StateEntered = (OnStateEnter)Delegate.CreateDelegate(typeof(OnStateEnter), null, house.GetMethod(methodName));
            else
                StateEntered += (OnStateEnter)Delegate.CreateDelegate(typeof(OnStateEnter), null, house.GetMethod(methodName));
            
            if(!SerializeData.ContainsKey("StateEntered_" + house.ToString() + "_" + methodName))
                SerializeData.Add("StateEntered_" + house.ToString() + "_" + methodName, new DelegateData() { DelegateName = "StateEntered", MethodHouse = house, MethodName = methodName });
        }

        public void AddUpdateStateMethod(Type house, string methodName)
        {
            if (house == null || house.GetMethod(methodName) == null)
                return;
            if (StateUpdate == null)
                StateUpdate = (OnStateUpdate)Delegate.CreateDelegate(typeof(OnStateUpdate), null, house.GetMethod(methodName));
            else
                StateUpdate += (OnStateUpdate)Delegate.CreateDelegate(typeof(OnStateUpdate), null, house.GetMethod(methodName));
            if (!SerializeData.ContainsKey("StateUpdate_" + house.ToString() + "_" + methodName))
                SerializeData.Add("StateUpdate_" + house.ToString() + "_" + methodName, new DelegateData() { DelegateName = "StateUpdate", MethodHouse = house, MethodName = methodName });

        }

        public void AddExitStateMethod(Type house, string methodName)
        {
            if (house == null || house.GetMethod(methodName) == null)
                return;
            if (StateExited == null)
                StateExited = (OnStateExit)Delegate.CreateDelegate(typeof(OnStateExit), null, house.GetMethod(methodName));
            else
                StateExited += (OnStateExit)Delegate.CreateDelegate(typeof(OnStateExit), null, house.GetMethod(methodName));
            if (!SerializeData.ContainsKey("StateExited_" + house.ToString() + "_" + methodName))
                SerializeData.Add("StateExited_" + house.ToString() + "_" + methodName, new DelegateData() { DelegateName = "StateExited", MethodHouse = house, MethodName = methodName });
        }      

        public void BindDelegates()
        {            
            foreach(var m in SerializeData)
            {
                switch(m.Value.DelegateName)
                {
                    case "StateEntered":
                        AddEnterStateMethod(m.Value.MethodHouse, m.Value.MethodName);
                        break;
                    case "StateUpdate":
                        AddUpdateStateMethod(m.Value.MethodHouse, m.Value.MethodName);
                        break;
                    case "StateExited":
                        AddExitStateMethod(m.Value.MethodHouse, m.Value.MethodName);
                        break;
                }
            }
        }

        public void EnterState()
        {
            if(StateEntered != null)
                StateEntered.Invoke();            
        }
        public void UpdateState()
        {
            if(StateUpdate != null)
                StateUpdate.Invoke();
        }
        public void ExitState()
        {
            if(StateExited != null)
                StateExited.Invoke();
        }
    }
}
