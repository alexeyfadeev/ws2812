namespace LightBoardSample
{
    using System.Drawing;
    using System.Net;
    using RedAlliance.LightBoardLib;

    class Program
    {
        static void Main(string[] args)
        {
            var prog = new Program();
            prog.Run();
        }

        public void Run()
        {
            var manager = new LightBoardManager(64, 8, 128);

            using (var bmp = SaveImage("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/81/81f0106f666b80042c4b4d1c2ffc50d0f010d155_medium.jpg"))
            {
                manager.RenderBitmap(bmp);
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
