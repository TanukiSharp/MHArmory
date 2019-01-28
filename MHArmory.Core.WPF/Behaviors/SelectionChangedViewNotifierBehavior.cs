using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MHArmory.Core.WPF.Behaviors
{
    public interface ISelectionChangedViewNotifier
    {
        Action<object> SelectionChangedHandler { get; set; }
    }

    public static class SelectionChangedViewNotifierBehavior
    {
        public static bool GetIsAttached(ItemsControl target)
        {
            return (bool)target.GetValue(IsAttachedProperty);
        }

        public static void SetIsAttached(ItemsControl target, bool value)
        {
            target.SetValue(IsAttachedProperty, value);
        }

        public static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(
            "IsAttached",
            typeof(bool),
            typeof(SelectionChangedViewNotifierBehavior),
            new PropertyMetadata(OnIsAttachedChanged)
        );

        private static void OnIsAttachedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = (ItemsControl)sender;

            if ((bool)e.NewValue)
                itemsControl.DataContextChanged += ItemsControl_DataContextChanged;
            else
                itemsControl.DataContextChanged -= ItemsControl_DataContextChanged;
        }

        private static void ItemsControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = (ItemsControl)sender;
            var scrollViewer = VisualTreeHelper.GetChild(itemsControl, 0) as ScrollViewer;

            if (e.NewValue is ISelectionChangedViewNotifier notifier)
            {
                notifier.SelectionChangedHandler = (obj) =>
                {
                    if (obj == null)
                    {
                        scrollViewer.ScrollToHome();
                        return;
                    }

                    DependencyObject container = itemsControl.ItemContainerGenerator.ContainerFromItem(obj);

                    if (container == null)
                    {
                        scrollViewer.ScrollToHome();
                        return;
                    }

                    if (container is FrameworkElement element)
                        element.BringIntoView();
                };
            }
        }
    }
}
