using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MHArmory.Core.WPF.Controls
{
    public class CustomTreeView : TreeView
    {
        public Brush LineBrush
        {
            get { return (Brush)GetValue(LineBrushProperty); }
            set { SetValue(LineBrushProperty, value); }
        }

        public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(
            nameof(LineBrush),
            typeof(Brush),
            typeof(CustomTreeView),
            new PropertyMetadata(OnLinePenPropertyChanged)
        );

        public double LineThickness
        {
            get { return (double)GetValue(LineThicknessProperty); }
            set { SetValue(LineThicknessProperty, value); }
        }

        public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register(
            nameof(LineThickness),
            typeof(double),
            typeof(CustomTreeView),
            new PropertyMetadata(OnLinePenPropertyChanged)
        );

        private static void OnLinePenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((CustomTreeView)sender).UpdateLinePen();
        }

        private void UpdateLinePen()
        {
            linePen = new Pen(LineBrush, LineThickness);
            if (linePen.CanFreeze)
                linePen.Freeze();
        }

        private static void OnItemSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((CustomTreeView)sender).ResetRender();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            ResetRender();
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            ResetRender();
        }

        protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
        {
            base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
            ResetRender();
        }

        private void ResetRender()
        {
            itemSize = Size.Empty;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            foreach (object item in ItemContainerGenerator.Items)
            {
                var tvi = (TreeViewItem)ItemContainerGenerator.ContainerFromItem(item);

                if (itemSize.IsEmpty)
                    itemSize = ((FrameworkElement)tvi.Template.FindName("PART_Header", tvi)).DesiredSize;

                Draw(tvi, drawingContext);
            }
        }

        private Pen linePen = new Pen(Brushes.Gray, 3.0);
        private Size itemSize = Size.Empty;

        private void Draw(TreeViewItem parent, DrawingContext drawingContext)
        {
            var childCenter = new Point();

            Point parentCenter = parent.TranslatePoint(new Point(), this);
            parentCenter.Offset(itemSize.Width / 2.0, itemSize.Height / 2.0);

            bool needVerticalLine = false;
            double verticalMidPointX = 0.0;

            bool isFirst = true;
            bool hasChildren = false;

            foreach (object child in parent.ItemContainerGenerator.Items)
            {
                var tvi = (TreeViewItem)parent.ItemContainerGenerator.ContainerFromItem(child);
                if (tvi == null)
                    continue;

                childCenter = tvi.TranslatePoint(new Point(), this);
                childCenter.Offset(itemSize.Width / 2.0, itemSize.Height / 2.0);

                if (isFirst)
                {
                    drawingContext.DrawLine(linePen, parentCenter, childCenter);
                    isFirst = false;
                }
                else
                {
                    hasChildren = true;

                    if (needVerticalLine == false)
                    {
                        needVerticalLine = true;
                        verticalMidPointX = parentCenter.X + (childCenter.X - parentCenter.X) / 2.0;
                    }

                    drawingContext.DrawLine(
                        linePen,
                        new Point(verticalMidPointX, childCenter.Y),
                        childCenter
                    );
                }

                Draw(tvi, drawingContext);
            }

            if (hasChildren == false)
                return;

            drawingContext.DrawLine(
                linePen,
                new Point(verticalMidPointX, parentCenter.Y),
                new Point(verticalMidPointX, childCenter.Y + LineThickness / 2.0)
            );
        }
    }
}
