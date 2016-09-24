using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BetterStartPage.Control.View
{
    public class AutoGrid : Panel
    {
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            "Columns", typeof(int), typeof(AutoGrid), new PropertyMetadata(default(int), OnColumnsChanged, CoerceColumns));

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (Panel)d;
            grid.InvalidateArrange();
            grid.InvalidateVisual();
        }

        private static object CoerceColumns(DependencyObject d, object basevalue)
        {
            if ((int)basevalue < 1)
            {
                return 1;
            }
            return basevalue;
        }

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (InternalChildren.Count == 0)
            {
                return new Size(0, 0);
            }
            var columns = Columns;
            var columnWidth = availableSize.Width / columns;

            var column = 0;
            var y = 0.0;
            var maxHeight = 0.0;

            foreach (UIElement child in InternalChildren)
            {
                var childSize = new Size(columnWidth, double.PositiveInfinity);
                child.Measure(childSize);

                if (child.DesiredSize.Height > maxHeight)
                {
                    maxHeight = child.DesiredSize.Height;
                }

                column++;
                if (column >= columns)
                {
                    y += maxHeight;
                    maxHeight = 0;
                    column = 0;
                }
            }
            return new Size(availableSize.Width, y);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (InternalChildren.Count == 0)
            {
                return new Size(0, 0);
            }

            var columns = Columns;
            var columnWidth = finalSize.Width / columns;

            var column = 0;
            var y = 0.0;
            var maxHeight = 0.0;

            var childRect = new Rect(0, 0, columnWidth, 0);
            foreach (UIElement child in InternalChildren)
            {
                childRect.Y = y;
                childRect.Height = child.DesiredSize.Height;
                child.Arrange(childRect);
                
                childRect.X += childRect.Width;
                if (child.DesiredSize.Height > maxHeight)
                {
                    maxHeight = child.DesiredSize.Height;
                }
                column++;

                if (column >= columns)
                {
                    column = 0;
                    y += maxHeight;
                    maxHeight = 0;
                    childRect.X = 0;
                }
            }

            return finalSize;
        }
    }
}
