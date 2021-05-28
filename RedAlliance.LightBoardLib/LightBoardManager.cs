namespace RedAlliance.LightBoardLib
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using rpi_ws281x;
    using Unosquare.RaspberryIO;
    using Unosquare.WiringPi;

    public class LightBoardManager
    {
        private Controller _controller;
        private WS281x _rpi;
        private Pin _pin;

        public LightBoardManager()
        {
            this.Initialize();
        }

        public bool InvertMode => _pin == Pin.Gpio13 || _pin == Pin.Gpio19;

        public LightBoardManager(int screenWidth, int screenHeight, byte brightness, Pin pin)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            Brightness = brightness;
            _pin = pin;
            this.Initialize();
        }
        
        public int ScreenWidth { get; private set; } = 64;
        
        public int ScreenHeight { get; private set; } = 8;
        
        public byte Brightness { get; private set; } = 255;
        
        public int LedCount => (ScreenWidth + 1) * ScreenHeight;

        public void RenderBitmap(Bitmap bmp)
        {
            for (int i = 0; i < ScreenWidth; i++)
            {
                for (int j = 0; j < ScreenHeight; j++)
                {
                    this.SetPixel(bmp.GetPixel(i, j), i, j);
                }
            }

            _rpi.Render();
        }

        public void RenderBitmapData(List<Color[]> bmp, int offsetX, int offsetY)
        {
            for (int i = 0; i < ScreenWidth; i++)
            {
                for (int j = 0; j < ScreenHeight; j++)
                {
                    this.SetPixel(bmp[j + offsetY][i + offsetX], i, j);
                }
            }

            _rpi.Render();
        }

        public void RenderPixel(Color pixel, int x, int y)
        {
            for (int i = 0; i < ScreenWidth; i++)
            {
                for (int j = 0; j < ScreenHeight; j++)
                {
                    var color = (j == y && i == x) ? pixel : Color.Black;
                    this.SetPixel(color, i, j);
                }
            }

            _rpi.Render();
        }

        public void RenderPixel2(Color pixel, int x, int y)
        {
            this.SetPixel(pixel, x, y);
            _rpi.Render();
        }

        private int ReduceColorChanel(int chanel)
        {
            return Math.Max(0, (int)(((double)chanel * (double)Brightness / 510.0) - 20));
        }

        private void SetPixel(Color color, int x, int y)
        {
            int i = x;

            if (this.InvertMode)
            {
                if ((y < (ScreenHeight / 2) && (x % 2 == 1))
                    || (y >= (ScreenHeight / 2) && (x % 2 == 0)))
                {
                    i++;
                    if (i >= ScreenWidth)
                    {
                        return;
                    }
                }

                int y0 = y < (ScreenHeight / 2) ? y : (ScreenHeight - y - 1);
                int y1 = (ScreenHeight / 2) - 1 - y0;
                y = (x % 2 == 1) ? y1 : (ScreenHeight - 1 - y1);

                color = Color.FromArgb(255, ReduceColorChanel(color.R), ReduceColorChanel(color.G), ReduceColorChanel(color.B));
            }

            bool reverseMode = i % 2 == 1;
            var j = reverseMode ? (ScreenHeight - y - 1) : y;

            _controller.SetLED(i * ScreenHeight + j, color);
        }

        private void Initialize()
        {
            Pi.Init<BootstrapWiringPi>();
            var settings = Settings.CreateDefaultSettings();

            _controller = settings.AddController(LedCount, _pin, StripType.WS2812_STRIP, Brightness, false);
            _rpi = new WS281x(settings);
        }
    }
}
