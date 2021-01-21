namespace RedAlliance.LightBoardLib
{
    using System.Drawing;
    using rpi_ws281x;
    using Unosquare.RaspberryIO;
    using Unosquare.WiringPi;

    public class LightBoardManager
    {
        private Controller _controller;
        private WS281x _rpi;

        public LightBoardManager()
        {
            this.Initialize();
        }

        public LightBoardManager(int screenWidth, int screenHeight, byte brightness)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            Brightness = brightness;
            this.Initialize();
        }
        
        public int ScreenWidth { get; private set; } = 64;
        
        public int ScreenHeight { get; private set; } = 8;
        
        public byte Brightness { get; private set; } = 255;
        
        public int LedCount => ScreenWidth * ScreenHeight;

        public void RenderBitmap(Bitmap bmp)
        {
            for (int i = 0; i < ScreenWidth; i++)
            {
                bool reverseMode = i % 2 == 1;

                for (int j = 0; j < ScreenHeight; j++)
                {
                    var pixelY = reverseMode ? (ScreenHeight - j - 1) : j;

                    _controller.SetLED(i * ScreenHeight + j, bmp.GetPixel(i, pixelY));
                }
            }

            _rpi.Render();
        }

        private void Initialize()
        {
            Pi.Init<BootstrapWiringPi>();
            var settings = Settings.CreateDefaultSettings();

            _rpi = new WS281x(settings);
            _controller = settings.AddController(LedCount, Pin.Gpio18, StripType.WS2812_STRIP, ControllerType.PWM0, Brightness, false);
        }
    }
}
