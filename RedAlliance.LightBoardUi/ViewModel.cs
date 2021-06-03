namespace RedAlliance.LightBoardUi
{
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    public class ViewModel
    {
        private LightBoard lightBoard = new LightBoard();

        public ViewModel()
        {
            this.LoadItems();
        }
        
        public ObservableCollection<SongItem> SongItems { get; set; }

        public LightBoardSettings LightBoardSettings { get; set; }
        
        public SongItem SelectedItem { get; set; }
        
        public int? RunningIndex { get; private set; }

        public async Task ItemSelected(SongItem selectedItem)
        {
            this.SelectedItem = selectedItem;
            this.RunningIndex = null;
#if !DEBUG
            await this.lightBoard.ItemSelected(selectedItem);
#endif
        }
        
        public async Task ButtonClick(int buttonIndex)
        {
            if (this.RunningIndex == buttonIndex
                || (buttonIndex > 1 && string.IsNullOrWhiteSpace(this.SelectedItem.Folder2)))
            {
                this.RunningIndex = null;
#if !DEBUG
                await this.lightBoard.Stop();
#endif
            }
            else
            {
                this.RunningIndex = buttonIndex;
#if !DEBUG
                await this.lightBoard.Run(buttonIndex);
#endif
            }
        }

        public async Task FlashButtonClick()
        {
#if !DEBUG
            await this.lightBoard.Flash();
#endif
        }

        private void LoadItems()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            this.SongItems = config.GetSection("songs").Get<ObservableCollection<SongItem>>();
            this.LightBoardSettings = config.GetSection("lightBoard").Get<LightBoardSettings>();

#if !DEBUG
            this.lightBoard.Initialize(this.LightBoardSettings);
#endif
        }
    }
}