using DiskVisualizer.StartView;
using FileSystemExplorerWPF;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DiskVisualizer
{
    public class DiskView : Control
    {
        private ListBox _listbox;
        private Grid _driveView;
        private Grid _startView;
        private Border _scanButton;

        public ObservableCollection<DiskModel> Drives { get; set; } = new ObservableCollection<DiskModel>();
        public DriveViewModel _model { get; set; } = new DriveViewModel { Background = Brushes.Beige };

        public RelayCommand ListBoxLeftButtonup { get; set; }
        public RelayCommand BackCommand { get; set; }
        public RelayCommand ScanDrive { get; set; }

        public event EventHandler<FileExplorerDriveAnalyzeDoneEventArgs> ScanComplete;

        public DiskView()
        {
            this.Loaded += OnLoaded;
            ProcessDrives();
            ListBoxLeftButtonup = new RelayCommand(ListBox_LeftButtonup);
            BackCommand = new RelayCommand(BackClicked);
            ScanDrive = new RelayCommand(Scan_Drive);
            FileSystemExplorer.Instance.DriveAnalyzeDone += DriveAnalyzeDone;
            FileSystemExplorer.Instance.ProgressUpdated += FileSystemExplorer_ProgressUpdated;
            _model.ShowScanButtonText = true;
            DataContext = new { Drives = Drives, Model = _model, MouseCommand = ListBoxLeftButtonup, BackCommand = BackCommand,ScanDrive = ScanDrive };
        }

        private void ProcessDrives()
        {
            ColorGenerator colorGen = new ColorGenerator(6);

            foreach (var drive in DriveInfo.GetDrives())
            {
                Drives.Add(new DiskModel
                {
                    Name = drive.Name.TrimEnd(new[] { ':', '\\' }),
                    Background = colorGen.NextColor(),
                    SizeText = $"{(drive.TotalSize - drive.TotalFreeSpace).FormatDataSize()} / {drive.TotalSize.FormatDataSize()}"
                });
            }
        }

        private void Scan_Drive(object parameters)
        {
            var driveInfo = DriveInfo.GetDrives().Where(x => x.Name.Contains(_model.DriveName)).First();
            _model.ShowScanButtonText = false;
            FileSystemExplorer.Instance.ScanDrive(driveInfo.Name, driveInfo.TotalSize);
            
        }

        private void FileSystemExplorer_ProgressUpdated(object sender, ProgressUpdatedEventArgs e)
        {
            _model.Progress = $"{Math.Round(e.PercentDone, 2)}%";

            var newHeight = ((e.PercentDone / 100) * this.ActualHeight) ;
            var newWidth = ((e.PercentDone / 100) * this.ActualWidth);
            var sb = new Storyboard();
            DoubleAnimation HeightOutAni = new DoubleAnimation { From = _scanButton.ActualHeight, To = newHeight, Duration = TimeSpan.FromMilliseconds(800), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(HeightOutAni, _scanButton);
            Storyboard.SetTargetProperty(HeightOutAni, new PropertyPath(HeightProperty));

            DoubleAnimation WidthOutAni = new DoubleAnimation { From = _scanButton.ActualWidth, To = newWidth, Duration = TimeSpan.FromMilliseconds(800), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(WidthOutAni, _scanButton);
            Storyboard.SetTargetProperty(WidthOutAni, new PropertyPath(WidthProperty));

            sb.Children.Add(HeightOutAni);
            sb.Children.Add(WidthOutAni);
            sb.Begin();

            sb.Completed += (obj, evt) =>
            {
                _model.Height = newHeight;
                _model.Width = newWidth;
            };
           
        }

        private void BackClicked(object parameters)
        {
            var sb = new Storyboard();


            DoubleAnimation driveViewAni = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(200), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(driveViewAni, _driveView);
            Storyboard.SetTargetProperty(driveViewAni, new PropertyPath(OpacityProperty));

            DoubleAnimation startViewAni = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(200), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(startViewAni, _startView);
            Storyboard.SetTargetProperty(startViewAni, new PropertyPath(OpacityProperty));
            startViewAni.BeginTime = driveViewAni.Duration.TimeSpan;


            sb.Children.Add(startViewAni);
            sb.Children.Add(driveViewAni);

            sb.Begin();

            sb.Completed += (obj, evt) => { _driveView.Visibility = Visibility.Hidden; };

            _startView.Opacity = 1.0;
            _startView.Visibility = Visibility.Visible;
            _driveView.Visibility = Visibility.Hidden;
        }

        private void DriveAnalyzeDone(object sender, FileExplorerDriveAnalyzeDoneEventArgs e)
        {
            _model.Progress = "100%";
            AnimateExit(e);
            
        }

        private void AnimateExit(FileExplorerDriveAnalyzeDoneEventArgs e)
        {
            var sb = new Storyboard();

            var text = (TextBlock)Template.FindName("ProgressText", this); ;

            DoubleAnimation TextAni = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(800), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(TextAni, text);
            Storyboard.SetTargetProperty(TextAni, new PropertyPath(OpacityProperty));
            TextAni.BeginTime = TimeSpan.FromMilliseconds(400);

            DoubleAnimation HeightOutAni = new DoubleAnimation { From = _scanButton.ActualHeight - 30, To = this.ActualHeight, Duration = TimeSpan.FromMilliseconds(800), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(HeightOutAni, _scanButton);
            Storyboard.SetTargetProperty(HeightOutAni, new PropertyPath(HeightProperty));

            DoubleAnimation WidthOutAni = new DoubleAnimation { From = _scanButton.ActualWidth - 30, To = this.ActualWidth, Duration = TimeSpan.FromMilliseconds(800), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(WidthOutAni, _scanButton);
            Storyboard.SetTargetProperty(WidthOutAni, new PropertyPath(WidthProperty));

            DoubleAnimation HeightInAni = new DoubleAnimation { From = this.ActualHeight - 30, To = 0, Duration = TimeSpan.FromMilliseconds(600), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(HeightInAni, _scanButton);
            Storyboard.SetTargetProperty(HeightInAni, new PropertyPath(HeightProperty));
            HeightInAni.BeginTime = HeightOutAni.Duration.TimeSpan;

            DoubleAnimation WidthInAni = new DoubleAnimation { From = this.ActualWidth - 30, To = 0, Duration = TimeSpan.FromMilliseconds(600), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(WidthInAni, _scanButton);
            Storyboard.SetTargetProperty(WidthInAni, new PropertyPath(WidthProperty));
            WidthInAni.BeginTime = WidthOutAni.Duration.TimeSpan;

            sb.Children.Add(TextAni);
            sb.Children.Add(HeightOutAni);
            sb.Children.Add(WidthOutAni);
            sb.Children.Add(HeightInAni);
            sb.Children.Add(WidthInAni);

            sb.Completed += (obj, evnt) =>
            {
                ScanComplete(this, e);
                _driveView.Visibility = Visibility.Hidden;
                _startView.Visibility = Visibility.Visible;
                _model.ShowScanButtonText = true;
                sb.Remove();
            };
            sb.Begin();
        }

        private void ListBox_LeftButtonup(object parameters)
        {
            var listBoxItem = _listbox.SelectedItem as DiskModel;           
            _model.Background = listBoxItem.Background;
            _model.DriveName = listBoxItem.Name;
            _model.SizeText = listBoxItem.SizeText;
            _model.Height = 300;
            _model.Width = 300;

            var sb = new Storyboard();

            

            DoubleAnimation startViewAni = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(200), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(startViewAni, _startView);
            Storyboard.SetTargetProperty(startViewAni, new PropertyPath(OpacityProperty));

            DoubleAnimation driveViewAni = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(200), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(driveViewAni, _driveView);
            Storyboard.SetTargetProperty(driveViewAni, new PropertyPath(OpacityProperty));
            driveViewAni.BeginTime = startViewAni.Duration.TimeSpan;

            sb.Children.Add(startViewAni);
            sb.Children.Add(driveViewAni);

            sb.Begin();
            _driveView.Opacity = 0.0;

            sb.Completed += (obj, evt) => { _startView.Visibility = Visibility.Hidden; sb.Remove(); };

            _driveView.Visibility = Visibility.Visible;
            
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _scanButton = (Border)Template.FindName("ScanButton", this);
            _listbox = (ListBox)Template.FindName("ItemContainer", this);
            _driveView = (Grid)Template.FindName("DriveView", this);
            _startView = (Grid)Template.FindName("StartView", this);
        }

        static DiskView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiskView), new FrameworkPropertyMetadata(typeof(DiskView)));
        }
    }
}
