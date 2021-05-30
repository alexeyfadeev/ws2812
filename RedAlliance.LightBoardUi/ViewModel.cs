namespace RedAlliance.LightBoardUi
{
    using System.Collections.ObjectModel;
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

        public void ItemSelected(SongItem selectedItem)
        {
            this.SelectedItem = selectedItem;
            this.RunningIndex = null;
#if !DEBUG
            this.lightBoard.ItemSelected(selectedItem);
#endif
        }
        
        public void ButtonClick(int buttonIndex)
        {
            if (this.RunningIndex == buttonIndex
                || (buttonIndex > 1 && string.IsNullOrWhiteSpace(this.SelectedItem.Folder2)))
            {
                this.RunningIndex = null;
#if !DEBUG
                this.lightBoard.Stop();
#endif
            }
            else
            {
                this.RunningIndex = buttonIndex;
#if !DEBUG
                this.lightBoard.Run(buttonIndex);
#endif
            }
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