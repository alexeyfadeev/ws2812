namespace RedAlliance.LightBoardUi
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FolderMovier;
    using LightBoardLib;
    using rpi_ws281x;

    public class LightBoard
    {
        private CancellationTokenSource previousRunCancellationTokenSource;
        private LightBoardSettings settings;
        private IList<LightBoardManager> managers;
        private SongItem songItem;
        private FolderManager folderManager;
        private FolderManager folderManager2;
        private SemaphoreSlim lockerSemaphoreSlim = new SemaphoreSlim(1, 1);

        public void Initialize(LightBoardSettings settings)
        {
            this.settings = settings;

            this.managers = new[]
            {
                new LightBoardManager(this.settings.ScreenWidth, this.settings.ScreenHeight,
                    this.settings.Brightness, Pin.Gpio18)
            };
            
            var colors = new List<Color[]>();
            for (int j = 0; j < this.settings.ScreenHeight; j++)
            {
                colors.Add(Enumerable.Range(0, this.settings.ScreenWidth)
                    .Select(i => Color.FromArgb(255, ((i + 1) * 8 - 1) % 56, ((i + 1) * 8 - 1) % 56, ((i + 1) * 8 - 1) % 56)).ToArray());
            }
            
            foreach (var manager in managers)
            {
                manager.RenderBitmapData(colors, 0, 0);
            }
        }

        public async Task ItemSelected(SongItem item)
        {
            await this.Stop();
            this.songItem = item;
            
            var path = Path.Combine(this.settings.Path, this.songItem.Folder);
            this.folderManager = new FolderManager();
            await folderManager.Initialize(path);
            
            if (!string.IsNullOrWhiteSpace(this.songItem.Folder2))
            {
                var path2 = Path.Combine(this.settings.Path, this.songItem.Folder2);
                this.folderManager2 = new FolderManager();
                await folderManager2.Initialize(path2);
            }
            else
            {
                this.folderManager2 = null;
            }
        }

        public async Task Run(int folderManagerIndex)
        {
            if (folderManagerIndex > 1 && this.folderManager2 == null)
            {
                await this.Stop();
                return;
            }
            
            this.CancelTask();

            this.previousRunCancellationTokenSource = new CancellationTokenSource();
            var token = this.previousRunCancellationTokenSource.Token;

            Task.Run(() => this.TaskProc(folderManagerIndex, token), token);
        }

        public async Task Stop()
        {
            this.CancelTask();
            
            var colors = new List<Color[]>();
            for (int j = 0; j < this.settings.ScreenHeight; j++)
            {
                colors.Add(Enumerable.Range(0, this.settings.ScreenWidth)
                    .Select(i => Color.FromArgb(255, 0, 0, 0)).ToArray());
            }
            
            await this.lockerSemaphoreSlim.WaitAsync();
            
            foreach (var manager in managers)
            {
                manager.RenderBitmapData(colors, 0, 0);
            }
            
            this.lockerSemaphoreSlim.Release();
        }

        private void CancelTask()
        {
            if (this.previousRunCancellationTokenSource != null)
            {
                this.previousRunCancellationTokenSource.Cancel();
                this.previousRunCancellationTokenSource.Dispose();
                this.previousRunCancellationTokenSource = null;
            }
        }
        
        private async Task TaskProc(int folderManagerIndex, CancellationToken ct)
        {
            await this.lockerSemaphoreSlim.WaitAsync();

            var folderManager = folderManagerIndex > 1
                ? this.folderManager2
                : this.folderManager;
            
            var max = managers.Count - 1;
            
            while (!ct.IsCancellationRequested)
            {
                foreach (var bmp in folderManager.Bitmaps)
                {
                    foreach (var item in managers
                        .Select((manager, i) => new {manager, y = (max - i) * 8}))
                    {
                        if (ct.IsCancellationRequested)
                        {
                            this.lockerSemaphoreSlim.Release();
                            return;
                        }
                        
                        item.manager.RenderBitmapData(bmp, 0, item.y + this.settings.Offset);
                    }

                    Thread.Sleep(25);
                }
            }

            this.lockerSemaphoreSlim.Release();
        }
    }
}
