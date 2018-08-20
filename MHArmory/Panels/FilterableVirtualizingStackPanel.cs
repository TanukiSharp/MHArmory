using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MHArmory.ViewModels;

// https://blogs.msdn.microsoft.com/bencon/2006/01/06/iscrollinfo-in-avalon-part-i/
// https://blogs.msdn.microsoft.com/bencon/2006/01/07/iscrollinfo-in-avalon-part-ii/
// https://blogs.msdn.microsoft.com/bencon/2006/01/08/iscrollinfo-in-avalon-part-iii/
// https://blogs.msdn.microsoft.com/bencon/2006/12/09/iscrollinfo-tutorial-part-iv/

// https://blogs.msdn.microsoft.com/dancre/tag/VirtualizingTilePanel/

namespace MHArmory.Panels
{
    public interface IHasVisible
    {
        bool IsVisible { get; }
    }

    public interface IFilterableContainer
    {
        event EventHandler VisibleItemCountChanged;
        int VisibleItemCount { get; }
        IEnumerable<IHasVisible> Items { get; }
    }

    public class FilterableVirtualizingStackPanel : Panel, IScrollInfo
    {
        public IFilterableContainer FilterableContainer
        {
            get { return (IFilterableContainer)GetValue(FilterableContainerProperty); }
            set { SetValue(FilterableContainerProperty, value); }
        }

        public static IFilterableContainer GetFilterableContainer(FrameworkElement target)
        {
            return (IFilterableContainer)target.GetValue(FilterableContainerProperty);
        }

        public static void SetFilterableContainer(FrameworkElement target, IFilterableContainer value)
        {
            target.SetValue(FilterableContainerProperty, value);
        }

        public static readonly DependencyProperty FilterableContainerProperty = DependencyProperty.RegisterAttached(
            "FilterableContainer",
            typeof(IFilterableContainer),
            typeof(FilterableVirtualizingStackPanel),
            new PropertyMetadata(OnFilterableContainerChanged)
        );

        private static void OnFilterableContainerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var instance = (FilterableVirtualizingStackPanel)sender;

            instance.OnFilterableContainerChanged((IFilterableContainer)e.OldValue, false);
            instance.OnFilterableContainerChanged((IFilterableContainer)e.NewValue, true);
        }

        private void OnFilterableContainerChanged(IFilterableContainer instance, bool isAttach)
        {
            if (instance == null)
                return;

            if (isAttach)
            {
                instance.VisibleItemCountChanged += OnVisibleItemCountChanged;
                InvalidateMeasure();
            }
            else
                instance.VisibleItemCountChanged -= OnVisibleItemCountChanged;
        }

        private void OnVisibleItemCountChanged(object sender, EventArgs e)
        {
            InvalidateMeasure();
        }

        //===============================================================================================================

        public ScrollViewer ScrollOwner { get; set; }

        public bool CanVerticallyScroll { get; set; }
        public bool CanHorizontallyScroll { get; set; }

        public double ExtentWidth => extent.Width;
        public double ExtentHeight => extent.Height;

        public double ViewportWidth => viewport.Width;
        public double ViewportHeight => viewport.Height;

        public double HorizontalOffset => offset.X;
        public double VerticalOffset => offset.Y;

        private TranslateTransform renderTransform = new TranslateTransform();

        public FilterableVirtualizingStackPanel()
        {
            RenderTransform = renderTransform;
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - 1.0);
        }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + 1.0);
        }

        public void LineLeft()
        {
            throw new NotImplementedException();
        }

        public void LineRight()
        {
            throw new NotImplementedException();
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - viewport.Height);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + viewport.Height);
        }

        public void PageLeft()
        {
            throw new NotImplementedException();
        }

        public void PageRight()
        {
            throw new NotImplementedException();
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - 4.0 * SystemParameters.WheelScrollLines);
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + 4.0 * SystemParameters.WheelScrollLines);
        }

        public void MouseWheelLeft()
        {
            throw new NotImplementedException();
        }

        public void MouseWheelRight()
        {
            throw new NotImplementedException();
        }

        public void SetHorizontalOffset(double offset)
        {
            throw new NotImplementedException();
        }

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0.0 || viewport.Height >= extent.Height)
                offset = 0.0;
            else
            {
                if (offset + viewport.Height >= extent.Height)
                    offset = extent.Height - viewport.Height;
            }

            this.offset.Y = offset;

            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();

            renderTransform.Y = -offset;
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var element = visual as UIElement;

            if (element == null || itemHeight < 0.0)
                return rectangle;

            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] == visual)
                {
                    double itemTop = element.DesiredSize.Height * i;
                    double itemBottom = itemTop + itemHeight;

                    double top = offset.Y;
                    double bottom = viewport.Height + offset.Y;

                    if (itemTop < top)
                        SetVerticalOffset(itemTop);
                    else if (itemBottom > bottom)
                        SetVerticalOffset(itemBottom - viewport.Height);

                    return rectangle;
                }
            }

            throw new ArgumentException("Given visual is not in this Panel");
        }

        //===============================================================================================================

        private Size extent;
        private Size viewport;
        private Point offset;

        private double itemHeight = -1.0;

        protected override Size MeasureOverride(Size availableSize)
        {
            itemHeight = -1.0;

            if (FilterableContainer == null || Children.Count == 0)
                return base.MeasureOverride(availableSize);

            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            IItemContainerGenerator generator = itemsControl.ItemContainerGenerator;

            bool isFirst = true;
            int itemIndex = -1;

            GeneratorPosition startPos;

            foreach (IHasVisible item in FilterableContainer.Items)
            {
                itemIndex++;

                if (item.IsVisible == false)
                    continue;

                if (isFirst)
                {
                    startPos = generator.GeneratorPositionFromIndex(itemIndex);
                    isFirst = false;
                }

                // some logic with ItemContainerGenerator, work unfinished
            }

            // ------------------------------------------------------

            // bellow code is a different logic

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
                itemHeight = Math.Max(itemHeight, child.DesiredSize.Height);
            }

            var extent = new Size(
                availableSize.Width,
                itemHeight * FilterableContainer.VisibleItemCount);

            if (extent != this.extent)
            {
                this.extent = extent;

                if (ScrollOwner != null)
                    ScrollOwner.InvalidateScrollInfo();
            }

            if (availableSize != viewport)
            {
                viewport = availableSize;

                if (ScrollOwner != null)
                    ScrollOwner.InvalidateScrollInfo();
            }

            return extent;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (FilterableContainer == null)
                return base.ArrangeOverride(finalSize);

            double top = 0.0;

            foreach (UIElement child in Children)
            {
                child.Arrange(new Rect(0.0, top, finalSize.Width, itemHeight));
                top += itemHeight;
            }

            return finalSize;
        }
    }
}
