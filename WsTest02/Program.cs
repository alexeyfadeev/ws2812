namespace WsTest02
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Net;
    using System.Threading;
    using rpi_ws281x;
    using Unosquare.RaspberryIO;
    using Unosquare.WiringPi;

    class Program
    {
        private const int LedCount = 512;
        private const int Length = 42;
        private const int ScreenWidth = 64;
        private const int ScreenHeight = 8;

        static void Main(string[] args)
        {
            var prog = new Program();
            prog.Loop(args[0]);
        }

        private void Loop(string url)
        {
            Pi.Init<BootstrapWiringPi>();

            //The default settings uses a frequency of 800000 Hz and the DMA channel 10.
            var settings = Settings.CreateDefaultSettings();

            //Use 16 LEDs and GPIO Pin 18.
            //Set brightness to maximum (255)
            //Use Unknown as strip type. Then the type will be set in the native assembly.
            var controller = settings.AddController(LedCount, Pin.Gpio18, StripType.WS2812_STRIP, ControllerType.PWM0, 255, false);

            using (var rpi = new WS281x(settings))
            {
                /*
                while (true)
                {
                    for (int i = 0; i < LedCount; i++)
                    {
                        controller.SetLED(i, Color.FromArgb(255, 156, Math.Abs(255 - i) % 256));

                        if (i >= Length)
                        {
                            controller.SetLED(i - Length, Color.Black);
                        }

                        rpi.Render();
                        Thread.Sleep(5);
                    }

                    for (int i = LedCount - 1; i >= LedCount - Length; i--)
                    {
                        controller.SetLED(i, Color.Black);
                    }

                    rpi.Render();
                }
                */

                using (var bmp = SaveImage(url))
                {
                    var heightDiff = bmp.Height - ScreenHeight;
                    int k = 0;
                    int step = 1;

                    for (;; k += step)
                    {
                        if (k == heightDiff)
                        {
                            step = -1;
                            k--;
                        }
                        else if (k == -1)
                        {
                            step = 1;
                            k++;
                        }

                        for (int i = 0; i < ScreenWidth; i++)
                        {
                            bool reverseMode = i % 2 == 1;

                            for (int j = 0; j < ScreenHeight; j++)
                            {
                                controller.SetLED(i * ScreenHeight + j, bmp.GetPixel(i,
                                    reverseMode ? (ScreenHeight - j) + k : j + k));
                            }
                        }

                        rpi.Render();
                        Thread.Sleep(10);
                    }
                }

                
            }
        }

        public static Bitmap TakeScreenshot()
        {
            var bmpScreenCapture = new Bitmap(ScreenWidth, ScreenHeight);

            using (Graphics g = Graphics.FromImage(bmpScreenCapture))
            {
                g.CopyFromScreen(0, 0, 0, 0,
                    new Size(ScreenWidth, ScreenHeight), CopyPixelOperation.SourceCopy);
            }

            return bmpScreenCapture;
        }

        public Bitmap SaveImage(string url)
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead(url))
            {
                var bitmap = new Bitmap(stream);
                return bitmap;
            }

                /*
            using (var bitmap = new Bitmap(stream))
            {
                var resizedHeight = ScreenWidth * bitmap.Height / bitmap.Width;

                return ResizeImage(bitmap, ScreenWidth, resizedHeight);
            }
            */
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
