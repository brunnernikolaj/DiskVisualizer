using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeMapSharp
{
    public class TreeMapper
    {

        private List<RectangleTemp> _data;

        private double _tmpWidth, _tmpHeight;

        public List<RectangleTemp> BuildMap( double width, double height, IEnumerable<double> data)
        {
            _tmpWidth = width;
            _tmpHeight = height;

            if (data.Count() == 1)
            {
                return new List<RectangleTemp> { new RectangleTemp { Width = width, Height = height } };
            }

            var dataTotal = data.Sum();

            var valueScale = ((width * height) / dataTotal) / 100;

            _data = data.OrderByDescending(x => x).Select(x => new RectangleTemp { Value = x * valueScale }).ToList();

            var info = new TreeMapInfo();
            info.DrawVertically = DrawVertically(width, height);

            PlaceRectangle(info,width,height);

            return _data;       
        }

        private void PlaceRectangle(TreeMapInfo info, double width, double height) {
            while (info.End != _data.Count())
            {
                info.LastAspectRatio = FindAspectRatio(info.Start, info.End, info.DrawVertically);

                if ((info.LastAspectRatio > info.CurrentAspectRatio) || (info.LastAspectRatio < 1))
                {
                    double currX = 0;
                    double currY = 0;

                    // Lock the previous items in place
                    for (int n = info.Start; n < info.End; n++)
                    {
                        _data[n].X = info.OffsetX + currX;
                        _data[n].Y = info.OffsetY + currY;

                        if (info.DrawVertically)
                            currY += _data[n].Height;
                        else
                            currX += _data[n].Width;
                    }

                    if (info.DrawVertically)
                        info.OffsetX += _data[info.Start].Width;
                    else
                        info.OffsetY += _data[info.Start].Height;

                    _tmpWidth = width - info.OffsetX;
                    _tmpHeight = height - info.OffsetY;

                    info.DrawVertically = DrawVertically(_tmpWidth, _tmpHeight);
                    info.Start = info.End;
                    info.End = info.Start;
                    info.CurrentAspectRatio = 9999999;

                    continue;
                }
                else
                {
                    // Store newly calculate sizes
                    for (int n = info.Start; n <= info.End; n++)
                    {
                        _data[n].Width = _data[n].TempWidth;
                        _data[n].Height = _data[n].TempHeight;
                    }
                    info.CurrentAspectRatio = info.LastAspectRatio;
                    // try to draw another item
                    info.End++;
                }             
            }

            // Set each item in the positions in the remaining area
            double currX1 = 0;
            double currY1 = 0;

            for (int n = info.Start; n < info.End; n++)
            {
                _data[n].X = info.OffsetX + currX1;
                _data[n].Y = info.OffsetY + currY1;

                if (info.DrawVertically)
                    currY1 += _data[n].Height;
                else
                    currX1 += _data[n].Width;
            }
        }

        private double FindAspectRatio(int start, int end, bool vert)
        {
            double total = 0;
            double aspect = 0;
            double localWidth;
            double localHeight;

            for (int n = start; n <= end; n++)
            {
                total += _data[n].Value;
            }

            // Scale as needed for the width or height
            if (vert)
            {
                localWidth = total / _tmpHeight * 100;
                localHeight = _tmpHeight;
            }
            else
            {
                localHeight = total / _tmpWidth * 100;
                localWidth = _tmpWidth;
            }

            for (int n = start; n <= end; n++)
            {
                if (vert)
                {
                    _data[n].TempWidth = localWidth;
                    _data[n].TempHeight = (localHeight * (_data[n].Value / total));
                }
                else
                {
                    _data[n].TempWidth = (localWidth * (_data[n].Value / total));
                    _data[n].TempHeight = localHeight;
                }

                aspect = Math.Max(_data[n].TempHeight / _data[n].TempWidth, _data[n].TempWidth / _data[n].TempHeight);
            }

            return aspect;
        }

        private bool DrawVertically(double width, double height)
        {
            return width > height;
        }
    }
}
