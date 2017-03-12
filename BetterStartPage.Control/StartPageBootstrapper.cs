using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BetterStartPage.Control.Settings;
using BetterStartPage.Control.ViewModel;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BetterStartPage.Control
{
    extern alias Shell15;

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
            return (bool)element.GetValue(IsAttachedProperty);
        }

        private static void OnAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!_initialized)
            {
                Debug.Fail("How can the package not be initialized but the UI is created?");
                return;
            }

            var userControl = (UserControl)d;
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
                    Bootstrap2015(userControl);
                    break;
                case "15.0":
                default:
                    Bootstrap2017(userControl);
                    break;
            }
        }

        private static bool _initialized;
        public static void Initialize(IServiceProvider serviceProvider)
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;

            var dte = serviceProvider.GetService(typeof(DTE)) as DTE2;
            Ioc.Instance.RegisterInstance(dte);
            Ioc.Instance.Register<IIdeAccess, VsIdeAccess>();
            Ioc.Instance.RegisterInstance(serviceProvider);

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

        private static void Bootstrap2017(UserControl userControl)
        {
            try
            {
                var shell = Package.GetGlobalService(typeof(SVsShell)) as IVsShell;

                if (shell == null)
                {
                    return;
                }

                var shellInitHelper = new ShellInitHelper(shell, () =>
                {
                    try
                    {
                        userControl.Content = Application.LoadComponent(new Uri("Microsoft.VisualStudio.Shell.UI.Internal;component/packages/startpage/controls/start.xaml", UriKind.Relative));

                        VsResources.LinkStyleKey = VsBrushes.StartPageTextControlLinkSelectedKey;
                        VsResources.LinkHoverStyleKey = VsBrushes.StartPageTextControlLinkSelectedHoverKey;
                        VsResources.DirectoryLinkStyleKey = VsBrushes.StartPageTextHeadingKey;
                        VsResources.StartPageTabBackgroundKey = VsBrushes.StartPageTabBackgroundKey;
                        VsResources.GroupHeaderStyleKey = VisualStudio2017StartPage.TextBlockEnvironment122PercentFontSizeStyleKey;
                        VsResources.GroupHeaderForegroundKey = VisualStudio2017StartPage.StartPageTitleTextBrushKey;

                        VisualStudio2017StartPage.SetupDataContexts(userControl.Content);

                        var mainPanel = VisualStudio2017StartPage.MainPanel(userControl.Content);
                        if (mainPanel != null)
                        {
                            mainPanel.MaxWidth = double.PositiveInfinity;
                            mainPanel.Margin = new Thickness(10, 0, 10, 0);

                            // effect bar
                            mainPanel.ColumnDefinitions[0].Width = new GridLength(30, GridUnitType.Pixel);
                            mainPanel.ColumnDefinitions[0].MinWidth = 0;

                            // recent/favorites
                            mainPanel.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                            mainPanel.ColumnDefinitions[1].MinWidth = 400;
                            mainPanel.ColumnDefinitions[1].MaxWidth = double.PositiveInfinity;

                            // spacing
                            mainPanel.ColumnDefinitions[2].Width = new GridLength(30, GridUnitType.Pixel);
                            mainPanel.ColumnDefinitions[2].MinWidth = 0;

                            // open/new project
                            mainPanel.ColumnDefinitions[3].Width = GridLength.Auto;

                            // effect bar
                            mainPanel.ColumnDefinitions[4].Width = new GridLength(30, GridUnitType.Pixel);
                            mainPanel.ColumnDefinitions[4].MinWidth = 0;
                        }

                        var mruPanel = VisualStudio2017StartPage.MRUPanel(userControl.Content);
                        if (mruPanel != null)
                        {
                            var recentLabel = mruPanel.Children.OfType<Label>().FirstOrDefault();
                            if (recentLabel != null)
                            {
                                recentLabel.SetResourceReference(FrameworkElement.StyleProperty, VisualStudio2017StartPage.LabelEnvironment200PercentFontSizeStyleKey);
                                recentLabel.Margin = new Thickness(0, 0, 0, 30);
                            }
                        }

                        var gettingStarted = VisualStudio2017StartPage.GettingStartedControl(userControl.Content);
                        if (gettingStarted != null)
                        {
                            var favouritesPanel = new Grid();
                            favouritesPanel.RowDefinitions.Add(new RowDefinition
                            {
                                Height = GridLength.Auto
                            });
                            favouritesPanel.RowDefinitions.Add(new RowDefinition
                            {
                                Height = new GridLength(1, GridUnitType.Star)
                            });

                            var label = new TextBlock
                            {
                                Text = "Favorites",
                                Margin = new Thickness(0, -9, 0, 10)
                            };
                            label.SetResourceReference(System.Windows.Controls.Control.ForegroundProperty, VisualStudio2017StartPage.StartPageTitleTextBrushKey);
                            label.SetResourceReference(FrameworkElement.StyleProperty, VisualStudio2017StartPage.TextBlockEnvironment200PercentFontSizeStyleKey);
                            favouritesPanel.Children.Add(label);

                            var projectGroupsControl = new ProjectGroupsControl();
                            projectGroupsControl.Margin = new Thickness(8, 0, 8, 30);
                            Grid.SetRow(projectGroupsControl, 1);
                            favouritesPanel.Children.Add(projectGroupsControl);

                            gettingStarted.FindAncestor<Grid>().Children.Add(favouritesPanel);
                            gettingStarted.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch (Exception e)
                    {
                        ActivityLog.LogError("StartPage", e.ToString());
                    }
                });
                shellInitHelper.Setup();
            }
            catch (Exception e)
            {
                ActivityLog.LogError("StartPage", e.ToString());
            }
        }
    }
}
