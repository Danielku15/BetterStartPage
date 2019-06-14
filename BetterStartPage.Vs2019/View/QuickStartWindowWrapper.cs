using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BetterStartPage.Settings;
using BetterStartPage.View;
using Microsoft.VisualStudio.PlatformUI.GetToCode;
using Expression = System.Linq.Expressions.Expression;

namespace BetterStartPage.Vs2019
{
    class QuickStartWindow
    {
        private static readonly Assembly Assembly = Assembly.Load("Microsoft.VisualStudio.Shell.UI.Internal");
        private static readonly Type QuickStartWindowType = Assembly.GetType("Microsoft.VisualStudio.PlatformUI.GetToCode.QuickStartWindow");
        private static readonly Func<object, ICommand> GetCommandFromWorkflowHostViewModel = CompileGetCommandWorkflowHostViewModel();
        private static Func<object, ICommand> CompileGetCommandWorkflowHostViewModel()
        {
            var workflowHostViewModelType = Assembly.GetType("Microsoft.VisualStudio.PlatformUI.GetToCode.WorkflowHostViewModel");
            var closeCommandProperty =
                workflowHostViewModelType.GetProperty("Close", BindingFlags.Instance | BindingFlags.Public);

            var dataContextParameter = Expression.Parameter(typeof(object), "ctx");

            var cast = Expression.Convert(dataContextParameter, workflowHostViewModelType);
            var commandAccess = Expression.Property(cast, closeCommandProperty);

            return Expression.Lambda<Func<object, ICommand>>(
                commandAccess,
                dataContextParameter
            ).Compile();
        }

        private static readonly PropertyInfo InstanceProperty = QuickStartWindowType.GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly EventInfo DialogCreatedEvent = QuickStartWindowType.GetEvent("DialogCreated", BindingFlags.Static | BindingFlags.NonPublic);

        private Window _instance;

        public static event EventHandler DialogCreated
        {
            add
            {
                DialogCreatedEvent.AddMethod.Invoke(null, new object[] { value });
            }
            remove
            {
                DialogCreatedEvent.RemoveMethod.Invoke(null, new object[] { value });
            }
        }

        public QuickStartWindow(Window instance)
        {
            _instance = instance;
        }

        public static QuickStartWindow Instance
        {
            get
            {
                object instance = InstanceProperty.GetValue(null);
                if (instance is Window window)
                {
                    return new QuickStartWindow(window);
                }
                return null;
            }
        }

        internal void PatchDialog()
        {
            _instance.ResizeMode = ResizeMode.CanResize;

            var settingsProvider = Ioc.Instance.Resolve<ISettingsProvider>();

            _instance.ContentRendered += (sender, args) =>
            {
                if (_instance.Content is Border border)
                {
                    border.BorderThickness = new Thickness(0);
                }
            };

            _instance.LocationChanged += (sender, args) =>
            {
                settingsProvider.WriteDouble("QuickStartWindow.Left", _instance.Left);
                settingsProvider.WriteDouble("QuickStartWindow.Top", _instance.Top);
            };

            _instance.SizeChanged += (sender, args) =>
            {
                settingsProvider.WriteDouble("QuickStartWindow.Width", args.NewSize.Width);
                settingsProvider.WriteDouble("QuickStartWindow.Height", args.NewSize.Height);
            };

            var w = settingsProvider.ReadDouble("QuickStartWindow.Width");
            if (!double.IsNaN(w))
            {
                _instance.Width = w;
            }

            var h = settingsProvider.ReadDouble("QuickStartWindow.Height");
            if (!double.IsNaN(h))
            {
                _instance.Height = h;
            }

            var l = settingsProvider.ReadDouble("QuickStartWindow.Left");
            if (!double.IsNaN(l))
            {
                _instance.Left = l;
            }

            var t = settingsProvider.ReadDouble("QuickStartWindow.Top");
            if (!double.IsNaN(t))
            {
                _instance.Top = t;
            }

            if (LogicalTreeHelper.FindLogicalNode(_instance, "ProjectMruGrid") is Grid projectMruGrid
                && projectMruGrid.Parent is Grid parentGrid)
            {
                if (parentGrid.ColumnDefinitions.Count == 3)
                {
                    parentGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    parentGrid.ColumnDefinitions[1].Width = new GridLength(20, GridUnitType.Pixel);
                }

                var splitter = new Grid();
                splitter.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                splitter.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Pixel) });
                splitter.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                
                var projectFavouriteGrid = new Grid();
                projectFavouriteGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                projectFavouriteGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                splitter.Children.Add(projectFavouriteGrid);

                var openFavouriteLabel = new TextBlock
                {
                    Text = "Open favourite"
                };
                openFavouriteLabel.SetResourceReference(System.Windows.Controls.Control.FontSizeProperty, "VsFont.Environment155PercentFontSizeKey");
                openFavouriteLabel.SetResourceReference(System.Windows.Controls.Control.FontWeightProperty, "VsFont.Environment155PercentFontWeightKey");
                Grid.SetRow(openFavouriteLabel, 0);
                projectFavouriteGrid.Children.Add(openFavouriteLabel);

                var groupsControl = new ProjectGroupsControl();
                Grid.SetRow(groupsControl, 1);
                projectFavouriteGrid.Children.Add(groupsControl);
                

                parentGrid.Children.Remove(projectMruGrid);
                Grid.SetRow(projectMruGrid, 0);
                Grid.SetColumn(projectMruGrid, 2);
                splitter.Children.Add(projectMruGrid);

                parentGrid.Children.Add(splitter);
            }
        }

        public void CompleteWorkflow(Action pendingWork)
        {
            var e = new WorkflowCompletedEventArgs("VS.IDE.OpenSolution", "Get To Code workflow", pendingWork);
            GetCommandFromWorkflowHostViewModel(_instance.DataContext).Execute(e);
        }
    }
}
