using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;

namespace HomeSystem_11_10_2018
{
    public class GarageControl
    {
        protected ControlSystem System;

        public event EventHandler<GarageEventArgs> GarageStateChange;

        protected virtual void OnGarageStateChange(GarageEventArgs garage)
        {
            if (GarageStateChange != null)
            {
                GarageStateChange(this, garage);
            }
        }

        public GarageControl(ControlSystem control)
        {
            System = control;

            try
            {
                System.DigitalInputPorts[1].StateChange += new DigitalInputEventHandler(InputOne_StateChange);
                System.DigitalInputPorts[2].StateChange += new DigitalInputEventHandler(InputTwo_StateChange);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Unable to register digital inputs: {0}", e.Message);
            }

            if (System.SupportsRelay) 
            {
                foreach (var relay in System.RelayPorts)
                {
                    relay.Register();
                }
            }

            if (System.SupportsDigitalInput) 
            {
                foreach (var input in System.DigitalInputPorts)
                {
                    input.Register();
                }
            }
        }

        void InputOne_StateChange(DigitalInput digitalInput, DigitalInputEventArgs args)
        {
            OnGarageStateChange(new GarageEventArgs(digitalInput));
        }

        void InputTwo_StateChange(DigitalInput digitalInput, DigitalInputEventArgs args)
        {
            OnGarageStateChange(new GarageEventArgs(digitalInput));
        }

        protected void open()
        {
            pulse(System.RelayPorts[2]);
        }

        protected void close() 
        {
            pulse(System.RelayPorts[2]);
        }

        public void release()
        {
            open();
        }

        public void toggle()
        {
            if (System.DigitalInputPorts[1].State)
            {
                open();
            }
            else if (this.System.DigitalInputPorts[2].State)
            {
                close();
            }
        }

        private object pulse(Relay relay)
        {
            relay.Close();
            Thread.Sleep(100);
            relay.Open();
            return relay;
        }
    }
}