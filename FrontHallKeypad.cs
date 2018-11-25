using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Lighting;

namespace HomeSystem_11_10_2018
{
    public class FrontHallKeypad
    {
        protected ClwDelvexP Keypad;

        protected ControlSystem System;

        public FrontHallKeypad(ControlSystem system)
        {
            System = system;

            SetupKeypad(new ClwDelvexP(0x1B, System.ControllerRFGatewayDevice));

            System.GarageControl.GarageStateChange += new EventHandler<GarageEventArgs>(garage_GarageStateChange);
        }

        protected void SetupKeypad(ClwDelvexP keypad)
        {
            keypad.ButtonStateChange += new ButtonEventHandler(keypad_ButtonStateChange);

            keypad.DimmerUISettings.ParameterButtonLogic = eButtonLogic.Remote;

            keypad.Feedbacks[4].State = true;

            keypad.Register();

            Keypad = keypad;
        }

        void garage_GarageStateChange(object sender, GarageEventArgs e)
        {
            if (e.Status == GarageEventArgs.State.closed)
            {
                Keypad.Feedbacks[4].BlinkPattern = eButtonBlinkPattern.AlwaysOn;
            }
            else if (e.Status == GarageEventArgs.State.open)
            {
                Keypad.Feedbacks[4].BlinkPattern = eButtonBlinkPattern.SlowBlip;
            }
            else
            {
                Keypad.Feedbacks[4].BlinkPattern = eButtonBlinkPattern.FastBlink;
            }
        }

        void keypad_ButtonStateChange(GenericBase device, ButtonEventArgs args)
        {
            if ((args.NewButtonState == eButtonState.Pressed) && (args.Button.Name == eButtonName.Button4))  
            {
                System.GarageControl.toggle();
            }
        }
    }
}