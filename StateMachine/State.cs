using System;

namespace StateMachineUtil
{
    public class State
    {
        #region DelegateDeclerations
        public delegate void StateEventDelegate();
        #endregion

        #region Fields
        private string _name;

        private StateEventDelegate _onEntered;
        private StateEventDelegate _onExited;
        private StateEventDelegate _onUpdated;
        #endregion

        #region Properties
        public string Name
        {
            get
            {
                return _name;
            }
        }
        public StateEventDelegate OnStateEntered
        {
            set
            {
                if (_onEntered == null)
                    _onEntered = value;
                else
                    _onEntered += value;
            }
        }

        public StateEventDelegate OnStateExited
        {
            set
            {
                if (_onExited == null)
                    _onExited = value;
                else
                    _onExited += value;
            }
        }

        public StateEventDelegate OnStateUpdated
        {
            set
            {
                if (_onUpdated == null)
                    _onUpdated = value;
                else
                    _onUpdated += value;
            }
        }
        #endregion

        #region Constructors
        public State(string name)
        {
            _name = name;
        }
        #endregion

        #region Methods
        public void Enter()
        {
            if (_onEntered != null)
                _onEntered.Invoke();
        }

        public void Update()
        {
            if (_onUpdated != null)
                _onUpdated.Invoke();
        }

        public void Exit()
        {
            if (_onExited != null)
                _onExited.Invoke();
        }

        private string DelegateData(Delegate[] data)
        {
            var formatedData = "";
            foreach (var method in data)
            {
                formatedData += "Target: " + method.Target + "\n" + "Method: " + method.Method + "\n";
            }
            return formatedData;
        }
        #endregion

        #region Overloads
        public override bool Equals(object obj)
        {
            if(obj.GetType() == typeof(State))
            {
                return (obj as State)._name == this._name;
            }
            return false;
        }

        public override string ToString()
        {
            var data = _name + '\n';
            if(_onEntered != null)
            {
                data += "OnStateEntered Methods:\n";
                data += DelegateData(_onEntered.GetInvocationList());
            }
            if(_onExited != null)
            {
                data += "OnStateExited Methods:\n";
                data += DelegateData(_onExited.GetInvocationList());
            }
            if(_onUpdated != null)
            {
                data += "OnStateUpdated Methods:\n";
                data += DelegateData(_onUpdated.GetInvocationList());
            }
            return data;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
