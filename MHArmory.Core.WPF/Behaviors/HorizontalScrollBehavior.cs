using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MHArmory.Core.WPF.Behaviors
{
    // Found here: https://stackoverflow.com/questions/3727439/how-to-enable-horizontal-scrolling-with-mouse
    // Slightly modified however.

    public static class HorizontalScrollBehavior
    {
        public static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(
            "IsAttached",
            typeof(bool),
            typeof(HorizontalScrollBehavior),
            new PropertyMetadata(false, OnIsAttachedPropertyChanged)
        );

        private static void OnIsAttachedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var element = (UIElement)sender;

            if ((bool)e.NewValue)
                element.PreviewMouseWheel += OnPreviewMouseWheel;
            else
                element.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = FindDescendant<ScrollViewer>((UIElement)sender);

            if (scrollViewer == null)
                return;

            if (Keyboard.Modifiers != ModifierKeys.Shift)
                return;

            int count = Math.Abs(e.Delta / 40);

            if (e.Delta < 0)
            {
                for (int i = 0; i < count; i++)
                    scrollViewer.LineRight();
            }
            else
            {
                for (int i = 0; i < count; i++)
                    scrollViewer.LineLeft();
            }

            e.Handled = true;
        }

        public static bool GetIsAttached(DependencyObject element)
        {
            return (bool)element.GetValue(IsAttachedProperty);
        }

        public static void SetIsAttached(DependencyObject element, bool value)
        {
            element.SetValue(IsAttachedProperty, value);
        }

        private static T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null)
                return null;

            if (obj is T)
                return obj as T;

            int childCount = VisualTreeHelper.GetChildrenCount(obj);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                T result = child as T ?? FindDescendant<T>(child);

                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
