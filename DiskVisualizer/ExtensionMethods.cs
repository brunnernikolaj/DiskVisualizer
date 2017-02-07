using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DiskVisualizer
{
    public static class ExtensionMethods
    {
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                if (child != null && child is T)
                {
                    yield return (T)child;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        public static T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
        {
            // Check if this object is the specified type
            if (obj is T)
                return obj as T;

            // Check for children
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            if (childrenCount < 1)
                return null;

            // First check all the children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return child as T;
            }

            // Then check the childrens children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = FindDescendant<T>(VisualTreeHelper.GetChild(obj, i));
                if (child != null && child is T)
                    return child as T;
            }

            return null;
        }

        public static T FindVisualChild<T>(DependencyObject depObj) where T : Visual
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        public static T FindVisualChild<T>(DependencyObject depObj, string name) where T : FrameworkElement
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T && (child as T).Name.Equals(name))
                    {
                        return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        if (childOfChild.Name.Equals(name))
                            return childOfChild;
                    }
                }
            }
            return null;
        }

        public static IEnumerable<T> Sort<T, X>(this IEnumerable<T> source, Func<T, X> sortBy, bool descendingOrder = false)
        {
            return descendingOrder ? source.OrderByDescending(sortBy) : source.OrderBy(sortBy);
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

        public static string FormatDataSize(this long sizeInBytes)
        {
            if (sizeInBytes < 1024)
                return sizeInBytes + " Bytes";

            if (sizeInBytes < 1048576)
                return (sizeInBytes / 1024) + " KB";

            if (sizeInBytes < 1073741824)
                return (sizeInBytes / 1048576) + " MB";

            return (sizeInBytes / 1073741824) + " GB";
        }

        public static string FormatDataSize(this double sizeInBytes)
        {
            if (sizeInBytes < 1024)
                return sizeInBytes + " Bytes";

            if (sizeInBytes < 1048576)
                return Math.Round((sizeInBytes / 1024),2) + " KB";

            if (sizeInBytes < 1073741824)
                return Math.Round((sizeInBytes / 1048576),2) + " MB";

            return Math.Round((sizeInBytes / 1073741824),2) + " GB";
        }

        public static Brush ChangeLightness(this Brush brush, float coef)
        {
            var color = (brush as SolidColorBrush).Color;
            return new SolidColorBrush( Color.FromArgb(color.A,(byte)(color.R * coef), (byte)(color.G * coef),
                (byte)(color.B * coef)));
        }

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
    }
}
