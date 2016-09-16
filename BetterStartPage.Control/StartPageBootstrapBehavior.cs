using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace BetterStartPage.Control
{
    public class StartPageBootstrapBehavior : Behavior<ContentPresenter>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            var startPage = (Grid)Application.LoadComponent(new Uri("Microsoft.VisualStudio.Shell.UI.Internal;component/StartPage.xaml",
                    UriKind.Relative));
            AssociatedObject.Content = startPage;

            var rightPanel = (ContentControl)startPage.FindName("rightpanel");
            if (rightPanel != null)
            {
                rightPanel.ContentTemplate = null;  
                rightPanel.Content = new ProjectGroupsControl();  
            }
        }
    }
}
