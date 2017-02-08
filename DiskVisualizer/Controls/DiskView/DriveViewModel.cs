using PropertyChanged;
using System.Windows.Media;

namespace DiskVisualizer
{
    [ImplementPropertyChanged]
    public class DriveViewModel
    {
        public Brush Background { get; set; }

        public string DriveName { get; set; }

        public string SizeText { get; set; }

        public string Progress { get; set; }

        public double Height { get; set; }

        public double Width { get; set; }

        public bool ShowScanButtonText { get; set; }
    }
}