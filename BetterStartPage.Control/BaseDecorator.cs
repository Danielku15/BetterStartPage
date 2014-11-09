using System.Windows;
using System.Windows.Controls;

namespace BetterStartPage.Control
{
    public abstract class BaseDecorator : Decorator
    {
        protected UIElement DecoratedUIElement
        {
            get
            {
                if (this.Child is BaseDecorator)
                {
                    return ((BaseDecorator)this.Child).DecoratedUIElement;
                }
                return this.Child;
            }
        }
    }
}