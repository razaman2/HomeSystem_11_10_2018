using System;
using Crestron.SimplSharpPro;

namespace HomeSystem_11_10_2018
{
    public class GarageEventArgs : EventArgs
    {
        public enum State
        {
            open, closed, opening, closing
        }

        private State _state;

        public State Status {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public GarageEventArgs(DigitalInput input)
        {
            if (input.State)
            {
                Status = (input.DeviceName.ToString().Equals("Port-01")) ? State.closed : State.open;
            }
            else
            {
                Status = (input.DeviceName.ToString().Equals("Port-02")) ? State.closing : State.opening;
            }
        }
    }
}