namespace LightBoardSample
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using RedAlliance.FolderMovier;
    using RedAlliance.LightBoardLib;
    using rpi_ws281x;

    class Program
    {
        static void Main(string[] args)
        {
            var prog = new Program();
            Console.WriteLine("Starting...");
            Console.ReadLine();
            prog.Run();
        }

        public void Run()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var folderSection = config.GetSection("imageFolder");

            var folderManager = new FolderManager();
            folderManager.Initialize(folderSection.Value).Wait();

            var brightness = (byte)int.Parse(config.GetSection("brightness").Value);
            var offset = int.Parse(config.GetSection("offset").Value);

            var managers = new[]
            {
                new LightBoardManager(96, 8, brightness, Pin.Gpio13),
                //new LightBoardManager(96, 8, brightness, Pin.Gpio13),
                new LightBoardManager(96, 8, brightness, Pin.Gpio21),
                new LightBoardManager(96, 8, brightness, Pin.Gpio18)
            };

            var colors = new List<Color[]>();
            for (int j = 0; j < 8; j++)
            {
                colors.Add(Enumerable.Range(0, 96).Select(i => Color.FromArgb(255, ((i + 1) * 8 - 1) % 256, ((i + 1) * 8 - 1) % 256, ((i + 1) * 8 - 1) % 256)).ToArray());
            }

            /*
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 10; x++)
            {
                managers[0].RenderPixel2(Color.DarkOrange, x, y);
                managers[1].RenderPixel2(Color.DarkOrange, x, y);
                Console.ReadLine();
            }
            */

            foreach (var manager in managers)
            {
                manager.RenderBitmapData(colors, 0, 0);
            }

            Console.ReadLine();

            var max = managers.Length - 1;

            while (true)
            {
                foreach (var bmp in folderManager.Bitmaps)
                {
                    foreach (var item in managers
                        .Select((manager, i) => new {manager, y = (max - i) * 8}))
                    {
                        item.manager.RenderBitmapData(bmp, 0, item.y + offset);
                        //Task.Factory.StartNew(() => item.manager.RenderBitmapData(bmp, 0, item.y));
                    }

                    Thread.Sleep(25);
                }
            }
        }

        private Bitmap SaveImage(string url)
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead(url))
            {
                var bitmap = new Bitmap(stream);
                return bitmap;
            }
        }
    }
}
