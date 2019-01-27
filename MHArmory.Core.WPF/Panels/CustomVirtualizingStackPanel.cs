using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MHArmory.Core.WPF.Panels
{
    public class CustomVirtualizingStackPanel : VirtualizingStackPanel
    {
        protected override Size MeasureOverride(Size constraint)
        {
            Size size = base.MeasureOverride(constraint);
            return size;
        }

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            UIElementCollection result = base.CreateUIElementCollection(logicalParent);
            return result;
        }

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
        }

        protected override bool ShouldItemsChangeAffectLayoutCore(bool areItemChangesLocal, ItemsChangedEventArgs args)
        {
            bool result = base.ShouldItemsChangeAffectLayoutCore(areItemChangesLocal, args);
            return result;
        }
    }
}
