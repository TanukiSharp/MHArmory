using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MHArmory.Core.WPF.Behaviors
{
    public static class DebugLayoutBehavior
    {
        public static bool GetIsAttached(FrameworkElement target)
        {
            return (bool)target.GetValue(IsAttachedProperty);
        }

        public static void SetIsAttached(FrameworkElement target, bool value)
        {
            target.SetValue(IsAttachedProperty, value);
        }

        public static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(
            "IsAttached",
            typeof(bool),
            typeof(FocusOnLoadBehavior),
            new PropertyMetadata(OnIsAttachedChanged)
        );

        private static void OnIsAttachedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = (FrameworkElement)sender;

            if (item.IsLoaded)
                OnAttachedElementLoaded(item);
            else
                item.Loaded += ItemLoaded;
        }

        private static void ItemLoaded(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            item.Loaded -= ItemLoaded;
            OnAttachedElementLoaded(item);
        }

        private static void OnAttachedElementLoaded(FrameworkElement item)
        {
            item.Name = "ROOT";
            ListenLayoutChanges(item);
        }

        private static void ListenLayoutChanges(DependencyObject item)
        {
            if (item is UIElement element)
                element.LayoutUpdated += (s, e) => OnLayoutUpdated(element, s, e);

            int childCount = VisualTreeHelper.GetChildrenCount(item);
            for (int i = 0; i < childCount; i++)
                ListenLayoutChanges(VisualTreeHelper.GetChild(item, i));
        }

        private static void OnLayoutUpdated(UIElement element, object sender, EventArgs e)
        {
            if (element is FrameworkElement fe)
            {
                if (string.IsNullOrWhiteSpace(fe.Name) == false)
                    Console.WriteLine(fe.Name);
            }
        }
    }
}
