using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MHArmory.Behaviors
{
    public static class FocusOnLoadBehavior
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
                MakeFocused(item);
            else
                item.Loaded += ItemLoaded;
        }

        private static void ItemLoaded(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            item.Loaded -= ItemLoaded;
            MakeFocused(item);
        }

        private static void MakeFocused(FrameworkElement item)
        {
            item.Focus();

            if (item is ListBox listBox)
            {
                DependencyObject listItem = listBox.ItemContainerGenerator.ContainerFromIndex(listBox.SelectedIndex);

                if (listItem is ListBoxItem listBoxItem)
                    listBoxItem.Focus();
            }
        }
    }
}
