using DiskVisualizer.StartView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiskVisualizer
{
    public class DiskView : Control
    {
        private ListBox _listbox;
        private Grid _driveView;

        public ObservableCollection<DiskModel> Drives { get; set; } = new ObservableCollection<DiskModel>();
        public DriveViewModel _model { get; set; } = new DriveViewModel { Background = Brushes.Beige };

        public RelayCommand ListBoxLeftButtonup { get; set; }
        public RelayCommand ScanDrive { get; set; }

        public event EventHandler ScanComplete;

        public DiskView()
        {
            this.Loaded += OnLoaded;
            ProcessDrives();
            ListBoxLeftButtonup = new RelayCommand(ListBox_LeftButtonup);
            ScanDrive = new RelayCommand(Scan_Drive);
            DataContext = new { Drives = Drives, Model = _model, MouseCommand = ListBoxLeftButtonup, ScanDrive = ScanDrive };
        }

        private void ProcessDrives()
        {
            ColorGenerator colorGen = new ColorGenerator(6);

            foreach (var drive in DriveInfo.GetDrives())
            {
                Drives.Add(new DiskModel
                {
                    Name = drive.Name.TrimEnd(new[] { ':', '\\' }),
                    BackgroundColor = colorGen.NextColor(),
                    SizeText = $"{(drive.TotalSize - drive.TotalFreeSpace).FormatDataSize()} / {drive.TotalSize.FormatDataSize()}"
                });
            }
        }

        private void Scan_Drive(object parameters)
        {
            var driveInfo = DriveInfo.GetDrives().Where(x => x.Name.Contains(_model.DriveName)).First();
            FileSystemExplorer.Instance.ScanDrive(driveInfo.Name, driveInfo.TotalSize);
            FileSystemExplorer.Instance.DriveAnalyzeDone += DriveAnalyzeDone;
        }

        private void DriveAnalyzeDone(object sender, FileExplorerDriveAnalyzeDoneEventArgs e)
        {
            ScanComplete(this, new EventArgs());
        }

        private void ListBox_LeftButtonup(object parameters)
        {
            var listBoxItem = _listbox.SelectedItem as DiskModel;           
            _model.Background = listBoxItem.BackgroundColor;
            _model.DriveName = listBoxItem.Name;
            _driveView.Visibility = Visibility.Visible;
            _listbox.Visibility = Visibility.Hidden;
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _listbox = (ListBox)Template.FindName("ItemContainer", this);
            _driveView = (Grid)Template.FindName("DriveView", this);
        }

        static DiskView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiskView), new FrameworkPropertyMetadata(typeof(DiskView)));
        }
    }
}
