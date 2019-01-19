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
    public static class ClickToCommandBehavior
    {
        public static int GetClickCount(DependencyObject target)
        {
            return (int)target.GetValue(ClickCountProperty);
        }

        public static void SetClickCount(DependencyObject target, int value)
        {
            target.SetValue(ClickCountProperty, value);
        }

        public static readonly DependencyProperty ClickCountProperty = DependencyProperty.RegisterAttached(
            "ClickCount",
            typeof(int),
            typeof(ClickToCommandBehavior),
            new PropertyMetadata(1)
        );

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
            typeof(ClickToCommandBehavior),
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
            typeof(ClickToCommandBehavior),
            new PropertyMetadata(OnCommandChanged)
        );

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                element.PreviewMouseLeftButtonDown += ElementMouseLeftButtonDown;
                element.Unloaded += ElementUnloaded;
            }
        }

        private static void ElementUnloaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;

            element.PreviewMouseLeftButtonDown -= ElementMouseLeftButtonDown;
            element.Unloaded -= ElementUnloaded;
        }

        private static void ElementMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;

            if (e.ClickCount == GetClickCount(element))
            {
                ICommand command = GetCommand(element);
                if (command == null)
                    return;

                command.ExecuteIfPossible(GetCommandParameter(element));
            }
        }
    }
}
