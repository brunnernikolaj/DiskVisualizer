using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DiskVisualizer.StartView
{
    [ImplementPropertyChanged]
    public class DiskModel
    {
        public string Name { get; set; }

        public string SizeText { get; set; }

        public SolidColorBrush BackgroundColor { get; set; }
    }
}
