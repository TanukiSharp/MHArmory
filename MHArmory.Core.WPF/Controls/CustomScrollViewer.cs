using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MHArmory.Core.WPF.Controls
{
    public class CustomScrollViewer : ScrollViewer
    {
        public bool IsScrollEnabled
        {
            get { return (bool)GetValue(IsScrollEnabledProperty); }
            set { SetValue(IsScrollEnabledProperty, value); }
        }

        public static bool GetIsScrollEnabled(DependencyObject target)
        {
            return (bool)target.GetValue(IsScrollEnabledProperty);
        }

        public static void SetIsScrollEnabled(DependencyObject target, bool value)
        {
            target.SetValue(IsScrollEnabledProperty, value);
        }

        public static readonly DependencyProperty IsScrollEnabledProperty = DependencyProperty.Register(
            nameof(IsScrollEnabled),
            typeof(bool),
            typeof(CustomScrollViewer),
            new PropertyMetadata(true)
        );

        //private ScrollBar verticalScrollBar = null;

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            base.OnTemplateChanged(oldTemplate, newTemplate);

            //if (newTemplate != null)
            //{
            //    ApplyTemplate();
            //    object lol = newTemplate.FindName("PART_VerticalScrollBar", this);
            //    verticalScrollBar = (ScrollBar)lol;
            //    Console.WriteLine(lol);
            //}
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (GetIsScrollEnabled(TemplatedParent) == false)
                e.Handled = true;
            else
                base.OnMouseWheel(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);


            //if (e.Property.Name.Contains("ScrollBar"))
            //{
            //    Console.WriteLine($"{e.Property.Name}: {e.OldValue} -> {e.NewValue}");
            //    if (verticalScrollBar != null)
            //        verticalScrollBar.Visibility = Visibility.Visible;
            //}
        }
    }
}
