using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiskVisualizer
{
    public class CustomSliderControl : Slider
    {
        protected override void OnRender(DrawingContext dc)
        {
            var textArray = new[] { "GB", "MB", "KB" };

            Size size = new Size(base.ActualWidth - 10, base.ActualHeight);
            int tickCount = (int)((this.Maximum - this.Minimum) / this.TickFrequency) + 1;
            if ((this.Maximum - this.Minimum) % this.TickFrequency == 0)
                tickCount -= 1;
            Double tickFrequencySize;
            // Calculate tick's setting
            tickFrequencySize = (size.Width * this.TickFrequency / (this.Maximum - this.Minimum));
            string text = "";
            FormattedText formattedText = null;
            double num = this.Maximum - this.Minimum;
            int i = 0;
            // Draw each tick text
            for (i = 0; i <= tickCount; i++)
            {
                text = textArray[i];

                formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 8, Brushes.Black);
                dc.DrawText(formattedText, new Point((tickFrequencySize * i), 20));

            }
        }

        static CustomSliderControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomSliderControl), new FrameworkPropertyMetadata(typeof(CustomSliderControl)));
        }
    }
}
