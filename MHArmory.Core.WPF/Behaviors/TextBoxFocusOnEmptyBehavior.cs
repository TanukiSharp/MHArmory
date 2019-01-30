using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MHArmory.Core.WPF.Behaviors
{
    public static class TextBoxFocusOnEmptyBehavior
    {
        public static bool GetIsAttached(TextBox target)
        {
            return (bool)target.GetValue(IsAttachedProperty);
        }

        public static void SetIsAttached(TextBox target, bool value)
        {
            target.SetValue(IsAttachedProperty, value);
        }

        public static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(
            "IsAttached",
            typeof(bool),
            typeof(TextBoxFocusOnEmptyBehavior),
            new PropertyMetadata(OnIsAttachedChanged)
        );

        private static void OnIsAttachedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = (TextBox)sender;

            if (item.IsLoaded)
                item.TextChanged += Item_TextChanged;
            else
                item.Loaded += ItemLoaded;
        }

        private static void ItemLoaded(object sender, RoutedEventArgs e)
        {
            var item = (TextBox)sender;
            item.Loaded -= ItemLoaded;
            item.TextChanged += Item_TextChanged;
        }

        private static void Item_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (textBox.Text.Length == 0)
                textBox.Focus();
        }
    }
}
