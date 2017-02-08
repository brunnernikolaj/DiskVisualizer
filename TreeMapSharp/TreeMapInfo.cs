using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeMapSharp
{
    class TreeMapInfo
    {
        public int Start { get; set; }
        public int End { get; set; }

        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        public double CurrentAspectRatio { get; set; } = 9999999;
        public double LastAspectRatio { get; set; }

        public bool DrawVertically { get; set; }
    }
}
