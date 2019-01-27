using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MHArmory.Core.WPF.Controls
{
    public class CustomListBox : ListBox
    {
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            bool result = base.IsItemItsOwnContainerOverride(item);
            return result;
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            DependencyObject result = base.GetContainerForItemOverride();

            return result;
        }

        protected override void OnItemContainerStyleChanged(Style oldItemContainerStyle, Style newItemContainerStyle)
        {
            base.OnItemContainerStyleChanged(oldItemContainerStyle, newItemContainerStyle);
        }

        protected override void OnItemContainerStyleSelectorChanged(StyleSelector oldItemContainerStyleSelector, StyleSelector newItemContainerStyleSelector)
        {
            base.OnItemContainerStyleSelectorChanged(oldItemContainerStyleSelector, newItemContainerStyleSelector);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            int index = ItemContainerGenerator.IndexFromContainer(element);
            Console.WriteLine(index);

            ////if (element is FrameworkElement frameworkElement)
            ////{
            //////    //frameworkElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //////    //frameworkElement.Arrange(new Rect(new Point(), frameworkElement.DesiredSize));

            //////    //maxItemHeight = Math.Max(maxItemHeight, frameworkElement.DesiredSize.Height);
            //////    //frameworkElement.Height = maxItemHeight;





            ////    frameworkElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            ////    if (frameworkElement.DesiredSize.Height > maxItemHeight)
            ////    {
            ////        maxItemHeight = frameworkElement.DesiredSize.Height;
            //////        this.InvalidateMeasure();
            //////        return;
            ////    }

            //////    frameworkElement.Arrange(new Rect(new Point(), new Size(frameworkElement.DesiredSize.Width, maxItemHeight)));
            //}
        }
    }
}
