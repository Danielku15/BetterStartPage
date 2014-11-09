using System;
using System.Windows;
using System.Windows.Controls;

namespace BetterStartPage.Control
{
    class AdvancedGrid : Grid
    {
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (VisualChildrenChanged != null)
            {
                VisualChildrenChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler VisualChildrenChanged;
    }
}
