using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskVisualizer
{
    public class FolderInfo
    {
        public long size { get; set; }

        public int depth { get; set; }

        public string name { get; set; }

        public string path { get; set; }

        public string parentFolder { get; set; }

        public long FileCount { get; set; }

        public long FolderCount { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
