using System;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace HomeSystem_11_10_2018
{
    public class Phone
    {
        ControlSystem System;
        CrestronApp Device;

        public Phone(ControlSystem system)
        {
            System = system;
            Setup();
        }

        protected void Setup()
        {
            Device = new CrestronApp(0x10, System);
            Device.ParameterProjectName.Value = "HomeMobile";

            Device.SigChange += new SigEventHandler(Phone_SigChange);
            System.GarageControl.GarageStateChange += new EventHandler<GarageEventArgs>(GarageControl_GarageStateChange);

            if (Device.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                ErrorLog.Notice("Unable to register your phone: {0}", Device.RegistrationFailureReason);
            }
        }

        void TempControl_TempEvents(object sender, TempEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void GarageControl_GarageStateChange(object sender, GarageEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void Phone_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Name)
            {
                case "HeatOn":
                    System.TempControl.on();
                    break;
                case "HeatOff":
                    System.TempControl.off();
                    break;
                case "HeatRaise":
                    System.TempControl.raise();
                    break;
                case "HeatLower":
                    System.TempControl.lower();
                    break;
            }

        }
    }
}