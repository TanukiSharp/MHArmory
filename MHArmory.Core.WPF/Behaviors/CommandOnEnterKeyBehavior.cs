using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MHArmory.Core.WPF.Behaviors
{
    public static class CommandOnEnterKeyBehavior
    {
        public static object GetCommandParameter(DependencyObject target)
        {
            return target.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(DependencyObject target, object value)
        {
            target.SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
            "CommandParameter",
            typeof(object),
            typeof(CommandOnEnterKeyBehavior),
            new PropertyMetadata(null)
        );

        public static ICommand GetCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(CommandOnEnterKeyBehavior),
            new PropertyMetadata(OnCommandChanged)
        );

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                element.PreviewKeyDown += ElementPreviewKeyDown;
                element.Unloaded += ElementUnloaded;
            }
        }

        private static void ElementPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var s = (DependencyObject)sender;

            if (e.Key == Key.Enter)
                GetCommand(s).ExecuteIfPossible(GetCommandParameter(s));
        }

        private static void ElementUnloaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;

            element.PreviewKeyDown -= ElementPreviewKeyDown;
            element.Unloaded -= ElementUnloaded;
        }
    }
}
