using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DiskVisualizer
{
    [ImplementPropertyChanged]
    public class TreemapValueModel
    {
        public string Text { get; set; }

        public string Path { get; set; }

        public double Value { get; set; }

        public double Height { get; set; }

        public double Width { get; set; }

        public bool IsDirectory { get; set; }

        public SolidColorBrush Background { get; set; }

        public Thickness Margin { get; set; }
    }
}
