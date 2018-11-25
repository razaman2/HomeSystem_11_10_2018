using System;
using Crestron.SimplSharp;

namespace HomeSystem_11_10_2018
{
    public class TempEventArgs : EventArgs
    {
        private int _setpoint = 60, _ambient = 0;

        private State _status = State.off;

        public enum State
        {
            on, off
        }

        protected virtual void OnTempStateChange()
        {
            if (TempStateChange != null)
            {
                if (Setpoint >= Ambient)
                {
                    _status = State.on;
                }
                else
                {
                    _status = State.off;
                }
                TempStateChange(this);
            }
        }

        public event Action<TempEventArgs> TempStateChange;


        public int Setpoint 
        { 
            get 
            { 
                return _setpoint; 
            } 
            set 
            {
                _setpoint = value;
                OnTempStateChange(); 
            } 
        }

        public int Ambient 
        { 
            get 
            { 
                return _ambient; 
            } 
            set 
            {
                _ambient = value;
                OnTempStateChange(); 
            } 
        }

        public State Status 
        { 
            get 
            { 
                return _status; 
            } 
            set 
            {
                _status = value;
                OnTempStateChange(); 
            } 
        }
    }
}