namespace WsTest01
{
    using System.Drawing;
    using rpi_ws281x;
    using Unosquare.RaspberryIO;
    using Unosquare.WiringPi;

    class Program
    {
        static void Main(string[] args)
        {
            var prog = new Program();
            prog.Loop();
        }

        private void Loop()
        {
            Pi.Init<BootstrapWiringPi>();

            //The default settings uses a frequency of 800000 Hz and the DMA channel 10.
            var settings = Settings.CreateDefaultSettings();

            //Use 16 LEDs and GPIO Pin 18.
            //Set brightness to maximum (255)
            //Use Unknown as strip type. Then the type will be set in the native assembly.
            var controller =
                settings.AddController(16, Pin.Gpio18, StripType.WS2812_STRIP, ControllerType.PWM0, 255, false);

            using (var rpi = new WS281x(settings))
            {
                //Set the color of the first LED of controller 0 to blue
                controller.SetLED(0, Color.Blue);
                //Set the color of the second LED of controller 0 to red
                controller.SetLED(1, Color.Red);
                rpi.Render();
            }
        }
    }
}
