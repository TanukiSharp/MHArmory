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
    public static class DoubleClickBehavior
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
            typeof(DoubleClickBehavior),
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
            typeof(DoubleClickBehavior),
            new PropertyMetadata(OnDoubleClickCommandChanged)
        );

        private static void OnDoubleClickCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Control control)
            {
                if (e.NewValue == null)
                    control.MouseDoubleClick -= ControlMouseDoubleClick;
                else
                    control.MouseDoubleClick += ControlMouseDoubleClick;
            }
            else
            {
                var element = (FrameworkElement)sender;
                if (element == null)
                    return;

                if (e.NewValue == null)
                    element.MouseLeftButtonDown -= ControlMouseLeftButtonDown;
                else
                    element.MouseLeftButtonDown += ControlMouseLeftButtonDown;
            }
        }

        private static void ControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                ControlMouseDoubleClick(sender, e);
        }

        private static void ControlMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;

            ICommand command = GetCommand(element);
            if (command == null)
                return;

            command.ExecuteIfPossible(GetCommandParameter(element));
        }
    }
}
