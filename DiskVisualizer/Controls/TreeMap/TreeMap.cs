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

        public event EventHandler OnBackButtonClicked;

        private TreeMapper _mapper = new TreeMapper();
        private Stack<string> _previousDirectories = new Stack<string>();
        private string _currentDir;

        private ListBox _listbox;
        private TreeMapFilter _currentFilter;

        public TreeMap()
        {
            _currentFilter = TreeMapFilter.GB;

            this.SizeChanged += OnResize;
            this.Loaded += OnLoaded;
            this.MouseRightButtonUp += TreeMap_MouseRightButtonUp;
            FileSystemExplorer.Instance.DeleteCompleted += FileSystemExplorer_DeleteCompleted;
            _model.BackButton = new RelayCommand(BackButtonClicked);
            _model.DeleteButton = new RelayCommand(DeleteButtonClicked,DeleteButtonCanExecute);
            _model.ListboxItemLeftButton = new RelayCommand(ListboxItem_MouseLeftButtonUp);
            _model.SliderValueChanged = new RelayCommand(Slider_ValueChanged);

            DataContext = new { Items = _nodes, Model = _model};
        }

        private void FileSystemExplorer_DeleteCompleted(object sender, EventArgs e)
        {
            _nodes.Clear();
            BuildTree(_currentDir);
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
            if (this.IsLoaded && this.Visibility == Visibility.Visible)
            {
                _nodes.Clear();
                BuildTree(_model.CurrentDir);
            }
        }

        private void BackButtonClicked(object parameters)
        {
            OnBackButtonClicked(this, new EventArgs());
        }

        private bool DeleteButtonCanExecute(object parameters)
        {
            var canExecute = _nodes.Any(x => x.IsSelected);
            
            return canExecute; 
        }

        private void DeleteButtonClicked(object parameters)
        {
            foreach (var item in _nodes.Where(x => x.IsSelected))
            {
                FileSystemExplorer.Instance.DeleteDirectoryAsync(item.Path);
            }

           
        }

        private void Slider_ValueChanged(object parameters)
        {
            switch (_model.SliderValue)
            {
                case 1:
                    _currentFilter = TreeMapFilter.GB;
                    break;
                case 2:
                    _currentFilter = TreeMapFilter.MB;
                    break;
                case 3:
                    _currentFilter = TreeMapFilter.KB;
                    break;
                default:
                    break;
            }
            _nodes.Clear();
            BuildTree(_currentDir);
        }

        private void ListboxItem_MouseLeftButtonUp(object sender)
        {
            var item = _listbox.SelectedItem as TreemapValueModel;

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                item.IsSelected = !item.IsSelected;
                _model.IsAItemSelected = _nodes.Any(x => x.IsSelected);
            }
            else if (item.IsDirectory)
            {
                _previousDirectories.Push(_model.CurrentDir);
                _model.CurrentDir = item.Path;
                _currentDir = _model.CurrentDir;

                var sb = AnimateExit();
                sb.Completed += (obj, evn) =>
                {
                    _nodes.Clear();
                    BuildTree(_model.CurrentDir);
                };
                sb.Begin();
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

            currentDirValues = currentDirValues
                .OrderByDescending(x => x.Value)
                .Where(x => x.Value < GetFilterSize(_currentFilter))
                .ToList();

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

            Storyboard sb = AnimateEnter(data);
            sb.Begin();
        }

        private static void ProcessCurrentDir(string path, List<TreemapValue> list)
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                try
                {
                    var info = FileSystemExplorer.Instance.FolderInfoDictionary[directory];
                    if (info != null)
                    {
                        list.Add(new TreemapValue { Name = info.name, Path = info.path, Value = info.size, IsDirectory = true });
                    }
                    
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

        private long GetFilterSize(TreeMapFilter filter)
        {
            switch (filter)
            {
                case TreeMapFilter.GB:
                    return 1073741824L * 1000;
                case TreeMapFilter.MB:
                    return 1048576 * 1000;
                case TreeMapFilter.KB:
                    return 1024 * 1000;
                default:
                    return 1073741824;
            }   
        }

        private Size MeasureString(string Text)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = Text;
            textBlock.FontSize = 12;
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

    enum TreeMapFilter
    {
        GB,MB,KB
    }
}
