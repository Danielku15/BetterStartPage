using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using BetterStartPage.Settings;
using BetterStartPage.View;
using Microsoft.VisualStudio.PlatformUI.GetToCode;
using Expression = System.Linq.Expressions.Expression;
using Application = System.Windows.Application;

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


        private static readonly Action<Window, double> SetInitialWidth = CompileSetFieldDouble("initialWidth");
        private static readonly Action<Window, double> SetInitialHeight = CompileSetFieldDouble("initialHeight");
        private static Action<Window, double> CompileSetFieldDouble(string fieldName)
        {
            var backingField = typeof(WorkflowHostView).GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (backingField == null)
            {
                return (w, d) => { };
            }

            var dynamicMethod = new DynamicMethod("SetField_" + fieldName,
                typeof(void),
                new[] { typeof(Window), typeof(double) },
                typeof(WorkflowHostView),
                true);

            var ilGenerator = dynamicMethod.GetILGenerator();

            // arg0.<field> = arg1
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, backingField);
            ilGenerator.Emit(OpCodes.Ret);

            return (Action<Window, double>)dynamicMethod.CreateDelegate(typeof(Action<Window, double>));
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
            _instance.WindowState = WindowState.Normal;
            _instance.SizeToContent = SizeToContent.Manual;

            var settingsProvider = Ioc.Instance.Resolve<ISettingsProvider>();

            _instance.ContentRendered += (sender, args) =>
            {
                if (_instance.Content is Border border)
                {
                    border.BorderThickness = new Thickness(0);
                }
            };

            _instance.SizeChanged += (sender, args) =>
            {
                SetInitialWidth(_instance, args.NewSize.Width);
                SetInitialHeight(_instance, args.NewSize.Height);
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

            var mainWindow = Application.Current?.MainWindow;
            if (mainWindow != null)
            {
                _instance.Left = mainWindow.Left + (mainWindow.ActualWidth - _instance.Width) / 2;
                _instance.Top = mainWindow.Top + (mainWindow.ActualHeight - _instance.Height) / 2;
            }
            else
            {
                _instance.Left = Screen.PrimaryScreen.WorkingArea.Left + (Screen.PrimaryScreen.WorkingArea.Width - _instance.Width) / 2;
                _instance.Top = Screen.PrimaryScreen.WorkingArea.Top + (Screen.PrimaryScreen.WorkingArea.Height - _instance.Height) / 2;
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
