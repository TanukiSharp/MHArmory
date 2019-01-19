using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MHArmory.Core.WPF.Behaviors
{
    public static class GreekLetterInputBehavior
    {
        public static bool GetIsAttached(DependencyObject target)
        {
            return (bool)target.GetValue(IsAttachedProperty);
        }

        public static void SetIsAttached(DependencyObject target, bool value)
        {
            target.SetValue(IsAttachedProperty, value);
        }

        public static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(
            "IsAttached",
            typeof(bool),
            typeof(GreekLetterInputBehavior),
            new PropertyMetadata(OnIsAttachedPropertyChanged));

        private static void OnIsAttachedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && sender is TextBox txt)
                txt.KeyDown += GreekLetterInputBehavior_KeyDown;
        }

        private static void GreekLetterInputBehavior_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (isCtrl && isShift)
            {
                var txt = (TextBox)sender;

                if (Keyboard.IsKeyDown(Key.A))
                    InsertText(txt, "α");
                else if (Keyboard.IsKeyDown(Key.B))
                    InsertText(txt, "β");
                else if (Keyboard.IsKeyDown(Key.Y))
                    InsertText(txt, "γ");
            }
        }

        private static void InsertText(TextBox txt, string textToInsert)
        {
            int secondStartIndex = txt.SelectionStart + txt.SelectionLength;
            int secondLength = txt.Text.Length - secondStartIndex;
            int newPosition = txt.SelectionStart + textToInsert.Length;

            string newText = $"{txt.Text.Substring(0, txt.SelectionStart)}{textToInsert}{txt.Text.Substring(secondStartIndex, secondLength)}";

            txt.Text = newText;
            txt.SelectionStart = newPosition;
            txt.SelectionLength = 0;
        }
    }
}
