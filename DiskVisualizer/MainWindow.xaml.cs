using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiskVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentDirectory;

        private Stack<string> previousDirectories = new Stack<string>();

        public MainWindow()
        {
            InitializeComponent();
            //var drive = DriveInfo.GetDrives().Where(x => x.Name == "E:\\").First();
            //currentDirectory = drive.Name;
            //FileSystemExplorer.ScanDrive(drive.Name, drive.TotalSize);
            //FileSystemExplorer.DriveAnalyzeDone += FileSystemExplorer_DriveAnalyzeDone;
            MainGrid.MouseRightButtonUp += MainGrid_MouseRightButtonUp;
            DiskView.ScanComplete += DiskView_ScanComplete;
        }

        private void DiskView_ScanComplete(object sender, EventArgs e)
        {
            DiskView.Visibility = Visibility.Hidden;
            TreeMap.Visibility = Visibility.Visible;
        }

        private void MainGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (previousDirectories.Count <= 0)
            {
                return;
            }

            currentDirectory = previousDirectories.Pop();

            var sb = new Storyboard();
            foreach (var item in MainGrid.Children.OfType<Border>())
            {
                //item.HorizontalAlignment = HorizontalAlignment.Center;
                //item.VerticalAlignment = VerticalAlignment.Center;
                DoubleAnimation ani = new DoubleAnimation { From = item.ActualHeight, To = 0, Duration = TimeSpan.FromMilliseconds(300) };
                Storyboard.SetTarget(ani, item);
                Storyboard.SetTargetProperty(ani, new PropertyPath(HeightProperty));

                DoubleAnimation ani2 = new DoubleAnimation { From = item.ActualWidth, To = 0, Duration = TimeSpan.FromMilliseconds(300) };
                Storyboard.SetTarget(ani2, item);
                Storyboard.SetTargetProperty(ani2, new PropertyPath(WidthProperty));

                sb.Children.Add(ani);
                sb.Children.Add(ani2);

            }

            sb.Completed += (obj, evn) =>
            {
                MainGrid.Children.Clear();
                BuildTree(currentDirectory);
            };
            sb.Begin();

        }

        private void FileSystemExplorer_DriveAnalyzeDone(object sender, FileExplorerDriveAnalyzeDoneEventArgs e)
        {

            BuildTree(e.Drivename);
        }

        private void BuildTree(string path)
        {
            var list = new List<TreemapValue>();

            foreach (var directory in Directory.GetDirectories(path))
            {
                try
                {
                    var info = FileSystemExplorer.Instance.FolderInfoDictionary[directory];
                    list.Add(new TreemapValue { Name = info.name, Path = info.path, Value = info.size,IsDirectory = true });
                }
                catch (Exception e)
                {

                }
            }
            foreach (var file in Directory.GetFiles(path))
            {
                var info = new FileInfo(file);
                list.Add(new TreemapValue { Name = info.Name, Value = info.Length });
            }

            TreeMapper mapper = new TreeMapper();
            ColorGenerator colorGeneraor = new ColorGenerator(10);
            int count = 0;
            list = list.OrderByDescending(x => x.Value).ToList();
            var data = mapper.BuildMap(MainGrid.ActualWidth, MainGrid.ActualHeight, list.Select(x => x.Value));

            foreach (var item in data)
            {
                var currentItem = list[count++];

                item.X = double.IsNaN(item.X) ? 0 : item.X;
                item.Y = double.IsNaN(item.Y) ? 0 : item.Y;

                Border border = new Border();

                border.Background = Brushes.Green;

                Grid newGrid = new Grid();
                //newGrid.Margin = new Thickness(2, 2, 2, 2);
                border.Margin = new Thickness(item.X, item.Y, 0, 0);
                border.Tag = currentItem.Path;
                if (currentItem.IsDirectory)
                {
                    border.MouseLeftButtonUp += Border_MouseLeftButtonUp;
                }
                border.Cursor = Cursors.Hand;
                border.MouseEnter += Border_MouseEnter;
                border.MouseLeave += Border_MouseLeave;
                border.BorderThickness = new Thickness(2);
                border.HorizontalAlignment = HorizontalAlignment.Left;
                border.VerticalAlignment = VerticalAlignment.Top;
                border.Background = colorGeneraor.NextColor();
                border.Height = item.Height;
                border.Width = item.Width;


                TextBlock text = new TextBlock();
                text.Text = currentItem.Name + "\n" + currentItem.Value.FormatDataSize();
                text.FontWeight = FontWeights.Light;
                text.FontFamily = new FontFamily("ClearType");
                text.TextAlignment = TextAlignment.Center;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;

                var textSize = MeasureString(text);

                if (textSize.Width < item.Width && textSize.Height < item.Height)
                {
                    newGrid.Children.Add(text);
                }

                border.Child = newGrid;
                MainGrid.Children.Add(border);
            }

            var sb = new Storyboard();
            foreach (var item in MainGrid.Children.OfType<Border>())
            {

                //item.HorizontalAlignment = HorizontalAlignment.Center;
                //item.VerticalAlignment = VerticalAlignment.Center;
                DoubleAnimation ani = new DoubleAnimation { From = 0, To = Double.IsNaN(item.Height) ? 0 : item.Height, Duration = TimeSpan.FromMilliseconds(300), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTarget(ani, item);
                Storyboard.SetTargetProperty(ani, new PropertyPath(HeightProperty));

                DoubleAnimation ani2 = new DoubleAnimation { From = 0, To = Double.IsNaN(item.Width) ? 0 : item.Width, Duration = TimeSpan.FromMilliseconds(300), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTarget(ani2, item);
                Storyboard.SetTargetProperty(ani2, new PropertyPath(WidthProperty));

                sb.Children.Add(ani);
                sb.Children.Add(ani2);

            }
            sb.Begin();

        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            border.Background = border.Background.ChangeLightness(1.1f);
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            border.Background = border.Background.ChangeLightness(0.9f);
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            previousDirectories.Push(currentDirectory);
            currentDirectory = border.Tag.ToString();

            var sb = new Storyboard();
            foreach (var item in MainGrid.Children.OfType<Border>())
            {
                //item.HorizontalAlignment = HorizontalAlignment.Center;
                //item.VerticalAlignment = VerticalAlignment.Center;
                DoubleAnimation ani = new DoubleAnimation { From = item.ActualHeight, To = 0, Duration = TimeSpan.FromMilliseconds(300), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn } };
                Storyboard.SetTarget(ani, item);
                Storyboard.SetTargetProperty(ani, new PropertyPath(HeightProperty));

                DoubleAnimation ani2 = new DoubleAnimation { From = item.ActualWidth, To = 0, Duration = TimeSpan.FromMilliseconds(300), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn } };
                Storyboard.SetTarget(ani2, item);
                Storyboard.SetTargetProperty(ani2, new PropertyPath(WidthProperty));

                sb.Children.Add(ani);
                sb.Children.Add(ani2);

            }

            sb.Completed += (obj, evn) =>
            {
                MainGrid.Children.Clear();
                BuildTree(currentDirectory);
            };
            sb.Begin();
        }

        private Size MeasureString(TextBlock textBlock)
        {
            var formattedText = new FormattedText(
                textBlock.Text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainGrid.Children.Clear();
            //BuildTree(currentDirectory);
        }

        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
             //BuildTree(currentDirectory);
        }

        private void MainGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var mousePos = e.GetPosition(sender as Grid);

            if (e.Delta < 0)
            {
                MainGrid.RenderTransform = new ScaleTransform(3, 3, mousePos.X, mousePos.Y); 
            }
            else
            {
                MainGrid.RenderTransform = new ScaleTransform();
            }
        }
    }
}
