using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MHArmory.Core.WPF.Panels
{
    public static class SizeExtensions
    {
        public static Size Expand(this Size size, Size additionnal)
        {
            return new Size(Math.Max(size.Width, additionnal.Width), Math.Max(size.Height, additionnal.Height));
        }
    }

    public class TooledContentPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Children.Count == 0)
                return base.MeasureOverride(availableSize);

            var result = new Size();

            if (Children.Count >= 1)
            {
                Children[0].Measure(availableSize);
                result = result.Expand(Children[0].DesiredSize);

                if (Children.Count >= 2)
                {
                    Children[1].Measure(availableSize);
                    result = result.Expand(Children[1].DesiredSize);

                    if (Children.Count > 2)
                    {
                        for (int i = 2; i < Children.Count; i++)
                            Children[i].Measure(availableSize);
                    }
                }
            }

            return result;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size remaining = base.ArrangeOverride(finalSize);

            if (Children.Count > 0)
            {
                if (Children.Count == 1)
                    Arrange(Children[0], new Rect(remaining));
                else if (Children.Count >= 2)
                {
                    Children[1].Measure(remaining);

                    double left = remaining.Width - Children[1].DesiredSize.Width;

                    Arrange(Children[1], new Rect(new Point(left, 0.0), new Size(Children[1].DesiredSize.Width, remaining.Height)));
                    Arrange(Children[0], new Rect(new Size(left, remaining.Height)));

                    if (Children.Count > 2)
                    {
                        for (int i = 2; i < Children.Count; i++)
                            Children[i].Arrange(Rect.Empty);
                    }
                }
            }

            return remaining;
        }

        private void Arrange(UIElement element, Rect available)
        {
            var fe = element as FrameworkElement;

            if (fe == null)
            {
                element.Arrange(available);
                return;
            }

            double xOffset = 0.0;
            double yOffset = 0.0;
            double width = 0.0;
            double height = 0.0;

            if (fe.HorizontalAlignment == HorizontalAlignment.Left)
            {
                width = Math.Min(fe.DesiredSize.Width, available.Width);
            }
            else if (fe.HorizontalAlignment == HorizontalAlignment.Center)
            {
                xOffset = (available.Width - fe.DesiredSize.Width) / 2.0;
                width = Math.Min(fe.DesiredSize.Width, available.Width);
            }
            else if (fe.HorizontalAlignment == HorizontalAlignment.Right)
            {
                xOffset = available.Width - fe.DesiredSize.Width;
                width = Math.Min(fe.DesiredSize.Width, available.Width);
            }
            else if (fe.HorizontalAlignment == HorizontalAlignment.Stretch)
            {
                width = available.Width;
            }

            if (fe.VerticalAlignment == VerticalAlignment.Top)
            {
                height = Math.Min(fe.DesiredSize.Height, available.Height);
            }
            else if (fe.VerticalAlignment == VerticalAlignment.Center)
            {
                yOffset = (available.Height - fe.DesiredSize.Height) / 2.0;
                height = Math.Min(fe.DesiredSize.Height, available.Height);
            }
            else if (fe.VerticalAlignment == VerticalAlignment.Bottom)
            {
                yOffset = available.Height - fe.DesiredSize.Height;
                height = Math.Min(fe.DesiredSize.Height, available.Height);
            }
            else if (fe.VerticalAlignment == VerticalAlignment.Stretch)
            {
                height = available.Height;
            }

            fe.Arrange(new Rect(new Point(available.Left + xOffset, available.Top + yOffset), new Size(width, height)));
        }
    }
}
