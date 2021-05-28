namespace RedAlliance.FolderMovier
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class FolderManager
    {
        public List<List<Color[]>> Bitmaps { get; private set; } = new List<List<Color[]>>();

        public async Task Initialize(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            var files = directoryInfo.GetFiles("*.jpg")
                .Concat(directoryInfo.GetFiles("*.png"))
                .OrderBy(x => x.Name)
                .ToList();

            foreach(var file in files)
            {
                AddBitmap(file.FullName);
            }
        }

        private void AddBitmap(string path)
        {
            var bitmapData = new List<Color[]>();
            const int bytesPerPixel = 4;

            using (var bmp = new Bitmap(path))
            {
                var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var srcData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                unsafe
                {
                    byte* srcPointer = (byte*) srcData.Scan0;

                    for (int i = 0; i < rect.Height; i++)
                    {
                        var bitmapLine = new Color[rect.Width];

                        for (int j = 0; j < rect.Width; j++)
                        {
                            bitmapLine[j] = Color.FromArgb(srcPointer[3], srcPointer[2], srcPointer[1], srcPointer[0]);
                            srcPointer += bytesPerPixel;
                        }

                        bitmapData.Add(bitmapLine);
                    }                    
                }
            }

            this.Bitmaps.Add(bitmapData);
        }
    }
}
