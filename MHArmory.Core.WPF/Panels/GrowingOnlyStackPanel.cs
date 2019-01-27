using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MHArmory.Core.WPF.Panels
{
    public class Lol : VirtualizingPanel
    {
    }

    public class GrowingOnlyStackPanel : StackPanel
    {
        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(
            nameof(GroupName),
            typeof(string),
            typeof(GrowingOnlyStackPanel),
            new PropertyMetadata(null)
        );

        private static readonly Dictionary<string, double> heights = new Dictionary<string, double>();

        //private static int level = 0;

        protected override Size MeasureOverride(Size availableSize)
        {
            string groupName = GroupName;

            if (groupName == null)
                return base.MeasureOverride(availableSize);

            //string space = new string(' ', level * 2);
            //Console.WriteLine($"{space}measure");
            //Console.WriteLine($"{space}{{");
            //level++;
            Size size = base.MeasureOverride(availableSize);
            //level--;
            //Console.WriteLine($"{space}}}");

            if (heights.TryGetValue(groupName, out double height) == false)
                heights[groupName] = size.Height;
            else
            {
                if (size.Height > height)
                {
                    height = size.Height;
                    heights[groupName] = height;

                    if (Parent != null)
                        ((FrameworkElement)Parent).InvalidateMeasure();
                }
            }

            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            //string space = new string(' ', level * 2);

            //Console.WriteLine($"{space}arrange");
            //Console.WriteLine($"{space}{{");
            //level++;
            Size size = base.ArrangeOverride(finalSize);
            //level--;
            //Console.WriteLine($"{space}}}");

            return size;
        }
    }
}
