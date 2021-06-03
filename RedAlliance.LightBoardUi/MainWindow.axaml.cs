namespace RedAlliance.LightBoardUi
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
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
            
            this.SongListBox.SelectionChanged += SongListBoxSelectionChanged;
            
            var button1 = this.FindControl<Button>("Button1");
            button1.Click += Button1Click;
            
            var button2 = this.FindControl<Button>("Button2");
            button2.Click += Button2Click;
            
            var flashButton = this.FindControl<Button>("FlashButton");
            flashButton.Click += FlashButtonClick;
        }

        private Image PreviewImage => this.FindControl<Image>("PreviewImage");

        private ListBox SongListBox => this.FindControl<ListBox>("SongListBox");

        public ViewModel ViewModel => (ViewModel)this.DataContext;
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void Button1Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            await this.ButtonClick(1);
        }

        private async void Button2Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            await this.ButtonClick(2);
        }

        private async void FlashButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            await this.ViewModel.FlashButtonClick();
        }

        private async void SongListBoxSelectionChanged(object? sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count < 1)
            {
                return;
            }

            var selectedItem = args.AddedItems[0] as SongItem;

            this.SongListBox.IsEnabled = false;
            await this.ViewModel.ItemSelected(selectedItem);
            this.SongListBox.IsEnabled = true;

            this.PreviewImage.Source = null;
        }

        private async Task ButtonClick(int buttonIndex)
        {
            if (this.ViewModel.SelectedItem == null)
            {
                return;
            }

            await this.ViewModel.ButtonClick(buttonIndex);

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