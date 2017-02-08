using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskVisualizer
{
    class TreemapValue
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public double Value { get; set; }

        public bool IsDirectory { get; set; }
    }
}
