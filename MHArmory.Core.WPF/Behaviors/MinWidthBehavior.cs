using System;
using System.Windows;

namespace MHArmory.Core.WPF.Behaviors
{
    public static class MinWidthBehavior
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
            typeof(MinWidthBehavior),
            new PropertyMetadata(OnIsAttachedChanged)
        );

        private static void OnIsAttachedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            item.SizeChanged += Item_SizeChanged;
        }

        private static void Item_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateElement(sender as FrameworkElement);
        }

        private static void UpdateElement(FrameworkElement item)
        {
            item.MinWidth = item.ActualWidth;
        }
    }
}
