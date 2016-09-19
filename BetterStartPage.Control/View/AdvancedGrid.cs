using System;
using System.Windows;
using System.Windows.Controls;

namespace BetterStartPage.Control.View
{
    internal class AdvancedGrid : Grid
    {
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            "Columns", typeof(int), typeof(AdvancedGrid), new PropertyMetadata(default(int), OnColumnsChanged));

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (AdvancedGrid)d;
            var newValue = Math.Max(1, (int)e.NewValue);

            if (grid.ColumnDefinitions.Count > newValue)
            {
                while (grid.ColumnDefinitions.Count > newValue)
                {
                    grid.ColumnDefinitions.RemoveAt(grid.ColumnDefinitions.Count - 1);
                }
            }
            else
            {
                while (grid.ColumnDefinitions.Count < newValue)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Star)
                    });
                }
            }
            RebuildGridChildren(grid);
        }

        public int Columns
        {
            get { return (int) GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
            "Rows", typeof(int), typeof(AdvancedGrid), new PropertyMetadata(default(int), OnRowsChanged));

        private static void OnRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (AdvancedGrid)d;
            var newValue = Math.Max(1, (int)e.NewValue);

            if (grid.RowDefinitions.Count > newValue)
            {
                while (grid.RowDefinitions.Count > newValue)
                {
                    grid.RowDefinitions.RemoveAt(grid.RowDefinitions.Count - 1);
                }
            }
            else
            {
                while (grid.RowDefinitions.Count < newValue)
                {
                    grid.RowDefinitions.Add(new RowDefinition
                    {
                        Height = GridLength.Auto
                    });
                }
            }
            RebuildGridChildren(grid);
        }

        public int Rows
        {
            get { return (int) GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            RebuildGridChildren(this);
        }

        private static void RebuildGridChildren(AdvancedGrid grid)
        {
            int column = 0;
            int row = 0;
            var columnCount = grid.Columns;
            for (int i = 0; i < grid.Children.Count; i++)
            {
                var child = grid.Children[i];
                if (child == null) continue;

                SetColumn(child, column);
                SetRow(child, row);

                column++;
                if (column >= columnCount)
                {
                    column = 0;
                    row++;
                }
            }

            grid.InvalidateArrange();
            grid.InvalidateVisual();
        }
    }
}
