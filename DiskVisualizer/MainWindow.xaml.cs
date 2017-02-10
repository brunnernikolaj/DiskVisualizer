using FileSystemExplorerWPF;
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
using TreeMapSharp;

namespace DiskVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DiskView.ScanComplete += DiskView_ScanComplete;
            TreeMap.OnBackButtonClicked += ScanNewDrive;
        }

        private void DiskView_ScanComplete(object sender, FileExplorerDriveAnalyzeDoneEventArgs e)
        {
            

            DiskView.Visibility = Visibility.Hidden;
            TreeMap.Visibility = Visibility.Visible;
            TreeMap.SetDirectory(e.Drivename);          
        }

        private void ScanNewDrive(object sender, EventArgs e)
        {
            DiskView.Visibility = Visibility.Visible;
            TreeMap.Visibility = Visibility.Hidden;


            DiskView.BackCommand.Execute(null);
        }
    }
}
