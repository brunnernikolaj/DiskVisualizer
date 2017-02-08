using FileSystemExplorerWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TreeMapSharp;

namespace DiskVisualizer
{
    public class TreeMap : Control
    {
        public ObservableCollection<TreemapValueModel> _nodes { get; set; } = new ObservableCollection<TreemapValueModel>();
        public TreeMapModel _model { get; set; } = new TreeMapModel();
        public RelayCommand BackButton { get; set; }
        public event EventHandler OnBackButtonClicked;

        private TreeMapper _mapper = new TreeMapper();
        private Stack<string> _previousDirectories = new Stack<string>();
        private string _currentDir;

        private ListBox _listbox;

        public TreeMap()
        {
            this.SizeChanged += OnResize;
            this.Loaded += OnLoaded;
            this.MouseRightButtonUp += TreeMap_MouseRightButtonUp;
            BackButton = new RelayCommand(BackButtonClicked);


            DataContext = new { Items = _nodes, Model = _model, BackButtonClicked = BackButton };
        }

        public void BackButtonClicked(object parameters)
        {
            OnBackButtonClicked(this, new EventArgs());
        }

        private void TreeMap_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_previousDirectories.Count <= 0)
            {
                return;
            }

            _model.CurrentDir = _previousDirectories.Pop();
            _currentDir = _model.CurrentDir;
            var sb = AnimateExit();
            sb.Completed += (obj, evn) =>
            {
                _nodes.Clear();
                BuildTree(_model.CurrentDir);
            };
            sb.Begin();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _listbox = (ListBox)Template.FindName("ItemContainer", this);
        }

        private void OnResize(object sender, SizeChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                _nodes.Clear();
                BuildTree(_model.CurrentDir);
            }
        }

        public void SetDirectory(string path)
        {
            _model.CurrentDir = path;
            _currentDir = path;
            _nodes.Clear();
            BuildTree(_model.CurrentDir);
        }

        private void BuildTree(string path)
        {
            var currentDirValues = new List<TreemapValue>();
            ProcessCurrentDir(path, currentDirValues);

            ColorGenerator colorGeneraor = new ColorGenerator(10);

            currentDirValues = currentDirValues.OrderByDescending(x => x.Value).ToList();
            var data = _mapper.BuildMap(_listbox.ActualWidth, _listbox.ActualHeight, currentDirValues.Select(x => x.Value));

            for (int i = 0; i < data.Count; i++)
            {
                var currentDirInfo = currentDirValues[i];
                var currentTreemapInfo = data[i];

                //Don't draw items that are less than a pixel
                if (currentTreemapInfo.Width < 1.0 || currentTreemapInfo.Height < 1.0)
                {
                    continue;
                }


                var newNode = new TreemapValueModel
                {
                    Height = currentTreemapInfo.Height,
                    Width = currentTreemapInfo.Width,
                    Margin = new Thickness(currentTreemapInfo.X, currentTreemapInfo.Y, 0, 0),
                    Background = colorGeneraor.NextColor(),
                    Value = currentTreemapInfo.Value,
                    Path = currentDirInfo.Path,
                    IsDirectory = currentDirInfo.IsDirectory

                };

                var text = $"{currentDirInfo.Name}\n{currentDirInfo.Value.FormatDataSize()}";

                //Only draw text if it fits within the item
                var textSize = MeasureString(text);
                if (textSize.Width < currentTreemapInfo.Width && textSize.Height < (currentTreemapInfo.Height / 2))
                {
                    newNode.Text = text;
                }
                else
                {
                    newNode.ToolTip = text;
                }

                _nodes.Add(newNode);
            }

            var listboxItems = ExtensionMethods.FindVisualChildren<ListBoxItem>(_listbox);
            foreach (var listboxItem in listboxItems)
            {
                if ((listboxItem.DataContext as TreemapValueModel).IsDirectory)
                {
                    listboxItem.MouseLeftButtonUp += ListboxItem_MouseLeftButtonUp;
                }
            }

            Storyboard sb = AnimateEnter(data);
            sb.Begin();
        }

        private void ListboxItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;
            _previousDirectories.Push(_model.CurrentDir);
            _model.CurrentDir = (item.DataContext as TreemapValueModel).Path;
            _currentDir = _model.CurrentDir;

            var sb = AnimateExit();
            sb.Completed += (obj, evn) =>
            {
                _nodes.Clear();
                BuildTree(_model.CurrentDir);
            };
            sb.Begin();
        }

        private Storyboard AnimateEnter(List<RectangleTemp> data)
        {
            var collection = ExtensionMethods.FindVisualChildren<ListBoxItem>(_listbox).ToList();
            var sb = new Storyboard();
            for (int i = 0; i < collection.Count(); i++)
            {
                var item = data[i];
                var listboxItem = collection[i];

                DoubleAnimation ani = new DoubleAnimation { From = 0, To = Double.IsNaN(item.Height) ? 0 : item.Height, Duration = TimeSpan.FromMilliseconds(250), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut } };
                Storyboard.SetTarget(ani, listboxItem);
                Storyboard.SetTargetProperty(ani, new PropertyPath(HeightProperty));

                DoubleAnimation ani2 = new DoubleAnimation { From = 0, To = Double.IsNaN(item.Width) ? 0 : item.Width, Duration = TimeSpan.FromMilliseconds(250), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut } };
                Storyboard.SetTarget(ani2, listboxItem);
                Storyboard.SetTargetProperty(ani2, new PropertyPath(WidthProperty));

                sb.Children.Add(ani);
                sb.Children.Add(ani2);
            }

            return sb;
        }

        private Storyboard AnimateExit()
        {
            var collection = ExtensionMethods.FindVisualChildren<ListBoxItem>(_listbox).ToList();
            var sb = new Storyboard();
            for (int i = 0; i < collection.Count(); i++)
            {
                var listboxItem = collection[i];
                var model = (listboxItem.DataContext as TreemapValueModel);
                DoubleAnimation ani = new DoubleAnimation { From = Double.IsNaN(model.Height) ? 0 : model.Height, To = 0, Duration = TimeSpan.FromMilliseconds(250), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut } };
                Storyboard.SetTarget(ani, listboxItem);
                Storyboard.SetTargetProperty(ani, new PropertyPath(HeightProperty));

                DoubleAnimation ani2 = new DoubleAnimation { From = Double.IsNaN(model.Width) ? 0 : model.Width, To = 0, Duration = TimeSpan.FromMilliseconds(250), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut } };
                Storyboard.SetTarget(ani2, listboxItem);
                Storyboard.SetTargetProperty(ani2, new PropertyPath(WidthProperty));

                sb.Children.Add(ani);
                sb.Children.Add(ani2);
            }

            return sb;
        }

        private static void ProcessCurrentDir(string path, List<TreemapValue> list)
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                try
                {
                    var info = FileSystemExplorer.Instance.FolderInfoDictionary[directory];
                    list.Add(new TreemapValue { Name = info.name, Path = info.path, Value = info.size, IsDirectory = true });
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
        }

        private Size MeasureString(string Text)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = Text;
            textBlock.FontWeight = FontWeights.Light;
            textBlock.FontFamily = new FontFamily("ClearType");
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;

            var formattedText = new FormattedText(
                textBlock.Text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }

        static TreeMap()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeMap), new FrameworkPropertyMetadata(typeof(TreeMap)));
        }
    }
}
