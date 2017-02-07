using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskVisualizer
{
    class FileObject
    {
        public string Name { get; set; }
        public long Size { get; set; }

        public DirectoryObject Parent { get; set; }
    }
}
