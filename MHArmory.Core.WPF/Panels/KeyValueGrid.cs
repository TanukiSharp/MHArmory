using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MHArmory.Core.WPF.Panels
{
    public class KeyValueGrid : Grid
    {
        private bool gridParametersInvalidated;

        public string KeyColumnSharedSizeGroup
        {
            get { return (string)GetValue(KeyColumnSharedSizeGroupProperty); }
            set { SetValue(KeyColumnSharedSizeGroupProperty, value); }
        }

        public static readonly DependencyProperty KeyColumnSharedSizeGroupProperty = DependencyProperty.Register(
            nameof(KeyColumnSharedSizeGroup),
            typeof(string),
            typeof(KeyValueGrid),
            new PropertyMetadata(null, OnKeyColumnSharedSizeGroupPropertyChanged)
        );

        public KeyValueGrid()
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.0, GridUnitType.Auto) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });
        }

        private static void OnKeyColumnSharedSizeGroupPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((KeyValueGrid)sender).ColumnDefinitions[0].SharedSizeGroup = (string)e.NewValue;
        }

        private void InvalidateGridParameters()
        {
            RowDefinitionCollection rowCollection = RowDefinitions;
            UIElementCollection children = Children;

            int remainder;
            int neededRowCount = Math.DivRem(children.Count, 2, out remainder);
            if (remainder > 0)
                neededRowCount++;

            int currentRowCount = rowCollection.Count;
            int deltaRowCount = neededRowCount - currentRowCount;

            if (deltaRowCount > 0)
            {
                for (int i = 0; i < deltaRowCount; i++)
                    rowCollection.Add(new RowDefinition { Height = new GridLength(0.0, GridUnitType.Auto) });
            }
            else if (deltaRowCount < 0)
            {
                rowCollection.RemoveRange(currentRowCount + deltaRowCount, -deltaRowCount);
            }

            int row = 0;
            int column = 0;
            foreach (UIElement element in children)
            {
                element.SetValue(Grid.ColumnProperty, column);
                element.SetValue(Grid.RowProperty, row);

                column++;
                if (column > 1)
                {
                    column = 0;
                    row++;
                }
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (gridParametersInvalidated)
            {
                gridParametersInvalidated = false;
                InvalidateGridParameters();
            }

            return base.MeasureOverride(constraint);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            gridParametersInvalidated = true;
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }
    }
}
