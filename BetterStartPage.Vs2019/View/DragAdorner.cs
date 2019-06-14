using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BetterStartPage.View
{
    internal class DragAdorner : Adorner
    {
        private readonly ContentPresenter _contentPresenter;
        private readonly AdornerLayer _adornerLayer;

        public DragAdorner(object data, DataTemplate dataTemplate, UIElement adornedElement, AdornerLayer adornerLayer)
            : base(adornedElement)
        {
            _adornerLayer = adornerLayer;

            _contentPresenter = new ContentPresenter { Content = data, ContentTemplate = dataTemplate };

            _adornerLayer.Add(this);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _contentPresenter.Measure(constraint);
            return new Size(((FrameworkElement) AdornedElement).ActualWidth,
                ((FrameworkElement)AdornedElement).ActualHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _contentPresenter;
        }

        protected override int VisualChildrenCount => 1;

        public void Destroy()
        {
            _adornerLayer.Remove(this);
        }
    }
}
