using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DiskVisualizer
{
    class DarkenColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float coef = 0.9f;
            var color = (value as SolidColorBrush).Color;
            return new SolidColorBrush(Color.FromArgb(color.A, (byte)(color.R * coef), (byte)(color.G * coef),
                (byte)(color.B * coef)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float coef = 1.1f;
            var color = (value as SolidColorBrush).Color;
            return new SolidColorBrush(Color.FromArgb(color.A, (byte)(color.R * coef), (byte)(color.G * coef),
                (byte)(color.B * coef)));
        }
    }
}
