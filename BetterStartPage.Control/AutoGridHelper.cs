using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BetterStartPage.Control
{
    class AutoGridHelper
    {
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached(
            "Columns", typeof(int), typeof(AutoGridHelper), new PropertyMetadata(default(int), OnColumnsChanged));

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (Grid)d;
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

        public static void SetColumns(DependencyObject element, int value)
        {
            element.SetValue(ColumnsProperty, value);
        }

        public static int GetColumns(DependencyObject element)
        {
            return (int)element.GetValue(ColumnsProperty);
        }

        public static readonly DependencyProperty RowsProperty = DependencyProperty.RegisterAttached(
            "Rows", typeof(int), typeof(AutoGridHelper), new PropertyMetadata(default(int), OnRowsChanged));

        private static void OnRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (Grid)d;
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

        public static void SetRows(DependencyObject element, int value)
        {
            element.SetValue(RowsProperty, value);
        }

        public static int GetRows(DependencyObject element)
        {
            return (int)element.GetValue(RowsProperty);
        }

        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached(
            "Attach", typeof (bool), typeof (AutoGridHelper), new PropertyMetadata(default(bool), OnAttach));

        private static void OnAttach(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (AdvancedGrid) d;
            grid.VisualChildrenChanged += (sender, args) => RebuildGridChildren(grid);
        }

        public static void SetAttach(DependencyObject element, bool value)
        {
            element.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject element)
        {
            return (bool) element.GetValue(AttachProperty);
        }

        private static void RebuildGridChildren(Grid grid)
        {
            int column = 0;
            int row = 0;
            var columnCount = GetColumns(grid);
            for (int i = 0; i < grid.Children.Count; i++)
            {
                var child = grid.Children[i];
                if (child == null) continue;

                Grid.SetColumn(child, column);
                Grid.SetRow(child, row);

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
