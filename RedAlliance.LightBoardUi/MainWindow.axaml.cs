namespace RedAlliance.LightBoardUi
{
    using System;
    using System.IO;
    using System.Linq;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using Avalonia.Media.Imaging;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.DataContext = new ViewModel();
            
            var songListBox = this.FindControl<ListBox>("SongListBox");
            songListBox.SelectionChanged += (s, args) => this.ItemSelected(args);
            
            var button1 = this.FindControl<Button>("Button1");
            button1.Click += (s, args) => this.ButtonClick(1);
            
            var button2 = this.FindControl<Button>("Button2");
            button2.Click += (s, args) => this.ButtonClick(2);
        }
        
        private Image PreviewImage => this.FindControl<Image>("PreviewImage");

        public ViewModel ViewModel => (ViewModel)this.DataContext;
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ItemSelected(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count < 1)
            {
                return;
            }
            
            var selectedItem = args.AddedItems[0] as SongItem;
            this.ViewModel.ItemSelected(selectedItem);

            this.PreviewImage.Source = null;
        }

        private void ButtonClick(int buttonIndex)
        {
            this.ViewModel.ButtonClick(buttonIndex);

            if (this.ViewModel.RunningIndex == null)
            {
                this.PreviewImage.Source = null;
                return;
            }
            
            string folder = Path.Combine(this.ViewModel.LightBoardSettings.Path,
                buttonIndex > 1
                    ? this.ViewModel.SelectedItem.Folder2
                    : this.ViewModel.SelectedItem.Folder);
            
            this.ShowImageFromFolder(folder);
        }

        private void ShowImageFromFolder(string folder)
        {
            var dirInfo = new DirectoryInfo(folder);
            var fileInfoList = dirInfo.GetFiles();
            var fileInfo = fileInfoList.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

            if (fileInfo != null)
            {
                using (var fileStream = fileInfo.OpenRead())
                {
                    var bitmap = new Bitmap(fileStream);
                    this.PreviewImage.Source = bitmap;
                }
            }
        }
    }
}