using PropertyChanged;
using System.Windows.Media;

namespace DiskVisualizer
{
    [ImplementPropertyChanged]
    public class DriveViewModel
    {
        public Brush Background { get; set; }

        public string DriveName { get; set; }
    }
}