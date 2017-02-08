using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemExplorerWPF
{
    public static class ExtensionMethods
    {
        public static List<string> ParentFoldersToList(this string path)
        {
            var split = path.Split('\\');
            var folderlist = new List<string>();

            for (var count = 0; count < split.Length - 2; count++)
            {
                var temp = split[0];
                for (var i = 0; i < split.Length - 2 - count; i++)
                {
                    temp += "\\" + split[i + 1];
                }
                folderlist.Add(temp);
            }
            return folderlist;
        }

        public static IEnumerable<KeyValuePair<T, Y>> FindFromList<T, Y>(this IEnumerable<KeyValuePair<T, Y>> source, List<string> folderStrings)
        {
            foreach (var entry in source)
            {
                foreach (var folderString in folderStrings.Where(folderString => folderString.Equals(entry.Key)))
                {
                    yield return entry;
                }
            }
        }
    }
}
