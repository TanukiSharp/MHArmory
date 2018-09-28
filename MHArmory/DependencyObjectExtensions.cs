using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MHArmory
{
    public static class DependencyObjectExtensions
    {
        public static T GetVisualParent<T>(this DependencyObject child) where T : Visual
        {
            while (child != null && (child is T) == false)
                child = VisualTreeHelper.GetParent(child);
            return child as T;
        }

        public static T FindVisualChild<T>(this DependencyObject root) where T : DependencyObject
        {
            T result = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(root);

            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);

                if (child == null)
                    continue;

                if (child is T typedChild)
                    return typedChild;

                result = FindVisualChild<T>(child);

                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
