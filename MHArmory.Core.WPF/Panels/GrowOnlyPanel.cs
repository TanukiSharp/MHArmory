using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MHArmory.Core.WPF.Panels
{
    public class GrowOnlyPanel : Panel
    {
        private double maxWidth = -1.0;

        protected override Size MeasureOverride(Size availableSize)
        {
            UIElement child = InternalChildren[0];

            child.Measure(availableSize);

            if (child.DesiredSize.Width > maxWidth)
            {
                maxWidth = child.DesiredSize.Width;
                InvalidateMeasure();
            }

            return new Size(maxWidth, child.DesiredSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            finalSize = new Size(maxWidth, finalSize.Height);

            UIElement child = InternalChildren[0];
            child.Arrange(new Rect(finalSize));

            return finalSize;
        }
    }
}
