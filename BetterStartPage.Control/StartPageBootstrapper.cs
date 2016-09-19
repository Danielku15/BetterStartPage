using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BetterStartPage.Control.Settings;
using BetterStartPage.Control.ViewModel;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace BetterStartPage.Control
{
    public class StartPageBootstrapper
    {
        public static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(
            "IsAttached", typeof(bool), typeof(StartPageBootstrapper), new PropertyMetadata(default(bool), OnAttached));
        
        public static void SetIsAttached(DependencyObject element, bool value)
        {
            element.SetValue(IsAttachedProperty, value);
        }

        public static bool GetIsAttached(DependencyObject element)
        {
            return (bool) element.GetValue(IsAttachedProperty);
        }

        private static void OnAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Initialize();
            var userControl = (UserControl) d;
            var dte = Ioc.Instance.Resolve<DTE2>();
            switch (dte.Version)
            {
                case "10.0":
                    Bootstrap2010(userControl);
                    break;
                case "11.0":
                    Bootstrap2012(userControl);
                    break;
                case "12.0":
                case "14.0":
                default:
                    Bootstrap2015(userControl);
                    break;
            }
        }

        private static bool _initialized;
        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            Ioc.Instance.RegisterInstance(dte);
            Ioc.Instance.Register<IIdeAccess, VsIdeAccess>();

            if (dte != null)
            {
                Ioc.Instance.Register<ISettingsProvider, VsSettingsProvider>();
            }
            else
            {
                Ioc.Instance.Register<IIdeAccess, DummySettingsProvider>();
            }

            Ioc.Instance.Register<ProjectGroupsViewModel>();
        }

        private static void Bootstrap2010(UserControl userControl)
        {
            var startPage = (Grid)Application.LoadComponent(new Uri("Microsoft.VisualStudio.Shell.UI.Internal;component/StartPage.xaml", UriKind.Relative));
            userControl.Content = startPage;

            VsResources.LinkStyleKey = VsBrushes.StartPageTextControlLinkSelectedKey;
            VsResources.LinkHoverStyleKey = VsBrushes.StartPageTextControlLinkSelectedHoverKey;
            VsResources.DirectoryLinkStyleKey = VsBrushes.StartPageTextHeadingKey;
            VsResources.StartPageTabBackgroundKey = VsBrushes.StartPageTabBackgroundKey;
            VsResources.GroupHeaderStyleKey = "StartPage.SubHeadingTextStyle";

            var layoutRoot = (Grid)startPage.FindName("LayoutRoot");
            foreach (var child in layoutRoot.Children.OfType<DependencyObject>())
            {
                var column = (int)child.GetValue(Grid.ColumnProperty);
                var container = child as Panel;
                if (column == 2 && container != null)
                {
                    container.HorizontalAlignment = HorizontalAlignment.Stretch;
                    container.VerticalAlignment = VerticalAlignment.Stretch;
                    container.Children.Clear();
                    container.Children.Add(new ProjectGroupsControl());
                }
            }
        }

        private static void Bootstrap2012(UserControl userControl)
        {
            var startPage = (Grid)Application.LoadComponent(new Uri("Microsoft.VisualStudio.Shell.UI.Internal;component/StartPage.xaml", UriKind.Relative));
            userControl.Content = startPage;

            VsResources.LinkStyleKey = VsBrushes.StartPageTextControlLinkSelectedKey;
            VsResources.LinkHoverStyleKey = VsBrushes.StartPageTextControlLinkSelectedHoverKey;
            VsResources.DirectoryLinkStyleKey = VsBrushes.StartPageTextHeadingKey;
            VsResources.StartPageTabBackgroundKey = VsBrushes.StartPageTabBackgroundKey;
            VsResources.GroupHeaderStyleKey = "StartPage.SubHeadingTextStyle";

            var layoutRoot = (Grid)startPage.FindName("LayoutRoot");
            foreach (var child in layoutRoot.Children.OfType<DependencyObject>())
            {
                var column = (int)child.GetValue(Grid.ColumnProperty);
                var container = child as Panel;
                if (column == 1 && container != null)
                {
                    container.HorizontalAlignment = HorizontalAlignment.Stretch;
                    container.VerticalAlignment = VerticalAlignment.Stretch;
                    container.Children.Clear();
                    container.Children.Add(new ProjectGroupsControl());
                }
            }
        }

        private static void Bootstrap2015(UserControl userControl)
        {
            var startPage = (Grid)Application.LoadComponent(new Uri("Microsoft.VisualStudio.Shell.UI.Internal;component/StartPage.xaml", UriKind.Relative));
            userControl.Content = startPage;

            VsResources.LinkStyleKey = VsBrushes.StartPageTextControlLinkSelectedKey;
            VsResources.LinkHoverStyleKey = VsBrushes.StartPageTextControlLinkSelectedHoverKey;
            VsResources.DirectoryLinkStyleKey = VsBrushes.StartPageTextHeadingKey;
            VsResources.StartPageTabBackgroundKey = VsBrushes.StartPageTabBackgroundKey;
            VsResources.GroupHeaderStyleKey = "StartPage.AnnouncementsHeadingTextStyle";

            var rightPanel = (ContentControl)startPage.FindName("rightpanel");
            if (rightPanel != null)
            {
                rightPanel.ContentTemplate = null;
                rightPanel.Content = new ProjectGroupsControl();
            }
        }
    }
}
