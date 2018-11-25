using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace HomeSystem_11_10_2018
{
    public class TempControl
    {
        protected TempEventArgs Temp;

        protected Relay Control;

        public event EventHandler<TempEventArgs> TempEvents;
        
        protected virtual void OnTempEvents(TempEventArgs obj)
        {
            if (TempEvents != null)
            {
                TempEvents(this, obj);
            }
        }

        public TempControl(Relay control)
        {
            Control = control;

            Temp = new TempEventArgs();

            Temp.TempStateChange += new Action<TempEventArgs>(Temp_TempStateChange);

            Control.StateChange += new RelayEventHandler(TempControl_StateChange);
        }

        void Temp_TempStateChange(TempEventArgs obj)
        {
            if (obj.Status == TempEventArgs.State.on)
            {
                on();
            }
            else if (obj.Status == TempEventArgs.State.off)
            {
                off();
            }
            OnTempEvents(obj);
        }

        void TempControl_StateChange(Relay relay, RelayEventArgs args)
        {
            ErrorLog.Notice("The Relay State Has Changed To: {0}", args.State.ToString());

            if (args.State)
            {
                Temp.Status = TempEventArgs.State.on;
            }
            else
            {
                Temp.Status = TempEventArgs.State.off;
            }
        }

        public void raise()
        {
            Temp.Setpoint++;
        }

        public void lower()
        {
            Temp.Setpoint--;
        }

        public void on() 
        {
            Control.Close();
        }

        public void off()
        {
            Control.Open();
        }

        public void ambient(int temp)
        {
            Temp.Ambient = temp;
        }
    }
}