using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DiskVisualizer
{
    class ColorGenerator
    {
        private int _numberOfColors;
        private List<SolidColorBrush> _colors = new List<SolidColorBrush>();
        private int _colorIndex;

        public ColorGenerator(int numOfColors)
        {
            _numberOfColors = numOfColors;

            _colors.Add(new SolidColorBrush(Color.FromRgb(129, 212, 250)));
            _colors.Add(new SolidColorBrush(Color.FromRgb(128, 222, 234)));
            _colors.Add(new SolidColorBrush(Color.FromRgb(128, 203, 196)));
            _colors.Add(new SolidColorBrush(Color.FromRgb(165, 214, 167)));
            _colors.Add(new SolidColorBrush(Color.FromRgb(197, 225, 165)));
            _colors.Add(new SolidColorBrush(Color.FromRgb(230, 238, 156)));
            _colors.Add(new SolidColorBrush(Color.FromRgb(255, 245, 157)));
            _colors.Add(new SolidColorBrush(Color.FromRgb(255, 224, 130)));
            _colors.Add(new SolidColorBrush(Color.FromRgb(255, 204, 128)));
            _colors.Add(new SolidColorBrush(Color.FromRgb(255, 171, 145)));


            //_colors = typeof(Brushes).GetProperties()
            //    .Select(x => x.GetValue(null) as SolidColorBrush)
            //    .Take(numOfColors)
            //    .ToList();
        }

        public SolidColorBrush NextColor()
        {
            if (_colorIndex == _numberOfColors)
                _colorIndex = 0;

            return _colors[_colorIndex++];          
        }
    }
}
