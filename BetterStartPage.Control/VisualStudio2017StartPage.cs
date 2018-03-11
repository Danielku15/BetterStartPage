extern alias Shell15;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Shell15::Microsoft.VisualStudio.Shell.Interop;
using Expression = System.Linq.Expressions.Expression;
using Task = System.Threading.Tasks.Task;

namespace BetterStartPage.Control
{
    extern alias Shell15;
    /// <summary>
    /// This class contains special logic for interacting with the VS2017 start page. 
    /// </summary>
    public static class VisualStudio2017StartPage
    {
        private static readonly Func<object, string, object> Start_GetTemplateChildInternal;

        private static readonly Func<object, object> CodeContainerProviderService_CodeContainerProviders;
        private static readonly Func<object, object> NewProjectsListViewModel_New;
        private static readonly Func<object, Task> NewProjectsListViewModel_SetMoreTemplatesTextRemoteSettingIfAvailableAsync;
        private static readonly Func<object, Task> NewProjectsListViewModel_LoadRecentTemplatesAsync;
        private static readonly Func<IServiceProvider, object, object> CodeContainerListViewModel_New;

        public static object StartPageTitleTextBrushKey
        {
            get { return Shell15.Microsoft.VisualStudio.PlatformUI.StartPageColors.StartPageTitleTextBrushKey; }
        }

        public static object TextBlockEnvironment200PercentFontSizeStyleKey
        {
            get { return Shell15.Microsoft.VisualStudio.Shell.VsResourceKeys.TextBlockEnvironment200PercentFontSizeStyleKey; }
        }

        public static object LabelEnvironment200PercentFontSizeStyleKey
        {
            get { return Shell15.Microsoft.VisualStudio.Shell.VsResourceKeys.LabelEnvironment200PercentFontSizeStyleKey; }
        }

        public static object TextBlockEnvironment122PercentFontSizeStyleKey
        {
            get { return Shell15.Microsoft.VisualStudio.Shell.VsResourceKeys.TextBlockEnvironment122PercentFontSizeStyleKey; }
        }

        static VisualStudio2017StartPage()
        {
            var uiInternalAssembly = Assembly.Load("Microsoft.VisualStudio.Shell.UI.Internal");
            var shellFrameworkAssembly = Assembly.Load("Microsoft.VisualStudio.Shell.Framework");
            var shell15Assembly = Assembly.Load("Microsoft.VisualStudio.Shell.15.0");
            var asyncServiceProviderType = shellFrameworkAssembly.GetType("Microsoft.VisualStudio.Shell.IAsyncServiceProvider");

            // Start_GetTemplateChildInternal
            {
                var startType = uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.Packages.StartPage.Start");

                var frameworkElementParameter = Expression.Parameter(typeof(object), "frameworkElement");
                var nameParameter = Expression.Parameter(typeof(string), "name");
                var getTemplateChildMethodGeneric = startType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                        .First(
                            m => m.Name == "GetTemplateChild" && m.IsGenericMethodDefinition)
                    ;
                var getTemplateChildMethod = getTemplateChildMethodGeneric.MakeGenericMethod(typeof(object));

                Start_GetTemplateChildInternal = Expression.Lambda<Func<object, string, object>>(
                    Expression.Call(Expression.TypeAs(frameworkElementParameter, startType), getTemplateChildMethod,
                        nameParameter),
                    frameworkElementParameter, nameParameter
                ).Compile();
            }

            // CodeContainerProviderService_CodeContainerProviders
            {
                var codeContainerProviderServiceType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.Services.CodeContainerProviderService");
                var codeContainerProvidersProperty =
                    codeContainerProviderServiceType.GetProperty("CodeContainerProviders",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                var codeContainerProviderServiceParameter = Expression.Parameter(typeof(object), "codeContainerProviderService");

                CodeContainerProviderService_CodeContainerProviders =
                    Expression.Lambda<Func<object, object>>(
                        Expression.Property(Expression.TypeAs(codeContainerProviderServiceParameter, codeContainerProviderServiceType), codeContainerProvidersProperty),
                        codeContainerProviderServiceParameter
                    ).Compile();
            }

            // NewProjectsListViewModel_New
            var newProjectsListViewModelType =
                uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.NewProjectsListViewModel");
            {
                var messageDialogInterfaceType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.StartPage.NewProject.IMessageDialog");
                var messageDialogType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.StartPage.NewProject.MessageDialog");
                var messageDialogConstructor = messageDialogType.GetConstructor(new Type[0]);

                var newProjectTelemetryLoggerInterfaceType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.Telemetry.INewProjectTelemetryLogger");
                var newProjectTelemetryLoggerType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.Telemetry.NewProjectTelemetryLogger");
                var newProjectTelemetryLoggerConstructor = newProjectTelemetryLoggerType.GetConstructor(new Type[0]);

                var newProjectsListViewModelConstructor =
                    newProjectsListViewModelType.GetConstructor(new[]
                    {
                        asyncServiceProviderType,
                        messageDialogInterfaceType,
                        newProjectTelemetryLoggerInterfaceType
                    });

                var asyncServiceProviderParameter = Expression.Parameter(typeof(object),
                    "asyncServiceProvider");

                NewProjectsListViewModel_New = Expression.Lambda<Func<object, object>>(
                    Expression.New(newProjectsListViewModelConstructor,
                        Expression.TypeAs(asyncServiceProviderParameter, asyncServiceProviderType),
                        Expression.New(messageDialogConstructor),
                        Expression.New(newProjectTelemetryLoggerConstructor)
                    ),
                    asyncServiceProviderParameter
                ).Compile();
            }

            // NewProjectsListViewModel_SetMoreTemplatesTextRemoteSettingIfAvailableAsync
            {
                var viewModelParameter = Expression.Parameter(typeof(object), "viewModel");

                var setMoreTemplatesTextRemoteSettingIfAvailableAsync =
                    newProjectsListViewModelType.GetMethod("SetMoreTemplatesTextRemoteSettingIfAvailableAsync",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                NewProjectsListViewModel_SetMoreTemplatesTextRemoteSettingIfAvailableAsync =
                    Expression.Lambda<Func<object, Task>>(
                        Expression.Call(Expression.TypeAs(viewModelParameter, newProjectsListViewModelType),
                            setMoreTemplatesTextRemoteSettingIfAvailableAsync),
                        viewModelParameter
                    ).Compile();
            }

            // NewProjectsListViewModel_LoadRecentTemplatesAsync
            {
                var viewModelParameter = Expression.Parameter(typeof(object), "viewModel");

                var loadRecentTemplatesAsync = newProjectsListViewModelType.GetMethod("LoadRecentTemplatesAsync",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                NewProjectsListViewModel_LoadRecentTemplatesAsync = Expression.Lambda<Func<object, Task>>(
                    Expression.Call(Expression.TypeAs(viewModelParameter, newProjectsListViewModelType),
                        loadRecentTemplatesAsync),
                    viewModelParameter
                ).Compile();
            }

            // CodeContainerListViewModel_New           
            {
                var codeContainerStorageManagerInterfaceType =
                    shellFrameworkAssembly.GetType("Microsoft.VisualStudio.Shell.CodeContainerManagement.ICodeContainerStorageManager");

                var codeContainerStorageManagerFactoryType =
                    shell15Assembly.GetType("Microsoft.VisualStudio.Shell.CodeContainerManagement.CodeContainerStorageManagerFactory");
                var codeContainerStorageManagerFactoryConstructor =
                    codeContainerStorageManagerFactoryType.GetConstructor(new[] { typeof(System.IServiceProvider) });
                var codeContainerStorageManagerFactoryCreate = codeContainerStorageManagerFactoryType.GetMethod(
                    "Create", BindingFlags.Instance | BindingFlags.Public);

                var codeContainerAccessManagerInterfaceType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.ICodeContainerAccessManager");
                var codeContainerAccessManagerType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.CodeContainerAccessManager");
                var codeContainerAccessManagerConstructor =
                    codeContainerAccessManagerType.GetConstructor(new[] { typeof(System.IServiceProvider) });
                if (codeContainerAccessManagerConstructor == null)
                {
                    codeContainerAccessManagerConstructor =
                        codeContainerAccessManagerType.GetConstructor(new[] { asyncServiceProviderType });
                }

                var removalPromptProviderInterfaceType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.IRemovalPromptProvider");
                var removalPromptProviderType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.RemovalPromptProvider");
                var removalPromptProviderConstructor =
                    removalPromptProviderType.GetConstructor(new[] { typeof(System.IServiceProvider) });

                var codeContainerIconMonikerProviderInterfaceType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.ICodeContainerIconMonikerProvider");
                var codeContainerIconMonikerProviderType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.CodeContainerIconMonikerProvider");
                var codeContainerIconMonikerProviderConstuctor =
                    codeContainerIconMonikerProviderType.GetConstructor(new[] { typeof(System.IServiceProvider) });

                var timeCategoryProviderInterfaceType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.ITimeCategoryProvider");
                var timeCategoryProviderType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.TimeCategoryProvider");
                var timeCategoryProviderInstanceProperty = timeCategoryProviderType.GetProperty("Instance",
                    BindingFlags.Static | BindingFlags.Public);

                var codeContainerTelemetryLoggerInterfaceType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.ICodeContainerTelemetryLogger");
                var codeContainerTelemetryLoggerType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.CodeContainerTelemetryLogger");
                var codeContainerTelemetryLoggerConstuctor = codeContainerTelemetryLoggerType.GetConstructors().First();
                var codeContainerTelemetryLoggerConstuctorParam = codeContainerTelemetryLoggerConstuctor.GetParameters().FirstOrDefault();
                Expression[] newCodeContainerTelemetryLoggerParams;
                if (codeContainerTelemetryLoggerConstuctorParam == null)
                {
                    newCodeContainerTelemetryLoggerParams = new Expression[0];
                }
                else if (codeContainerTelemetryLoggerConstuctorParam.ParameterType.FullName == "Microsoft.VisualStudio.PlatformUI.CodeContainerScenario")
                {
                    var startPageScenario = codeContainerTelemetryLoggerConstuctorParam.ParameterType.GetField("StartPage").GetValue(null);
                    newCodeContainerTelemetryLoggerParams = new Expression[]
                    {
                        Expression.Constant(startPageScenario)
                    };
                }
                else
                {
                    newCodeContainerTelemetryLoggerParams = new Expression[0];
                    Debug.Fail("unclear what parameter is expected, needs update");
                }

                var codeContainerListViewModelType =
                    uiInternalAssembly.GetType("Microsoft.VisualStudio.PlatformUI.CodeContainerListViewModel");
                var codeContainerListViewModelConstructor = codeContainerListViewModelType.GetConstructor(new[]
                {
                    codeContainerStorageManagerInterfaceType,
                    codeContainerAccessManagerInterfaceType,
                    removalPromptProviderInterfaceType,
                    codeContainerIconMonikerProviderInterfaceType,
                    timeCategoryProviderInterfaceType,
                    codeContainerTelemetryLoggerInterfaceType
                });

                var serviceProviderParameter = Expression.Parameter(typeof(System.IServiceProvider), "serviceProvider");
                var asyncServiceProviderParameter = Expression.Parameter(typeof(object), "asyncServiceProvider");

                var newCodeContainerAccessManagerConstructorParams =
                    codeContainerAccessManagerConstructor.GetParameters()[0].ParameterType == serviceProviderParameter.Type
                        ? (Expression)serviceProviderParameter
                        : Expression.TypeAs(asyncServiceProviderParameter, asyncServiceProviderType);


                CodeContainerListViewModel_New = Expression.Lambda<Func<System.IServiceProvider, object, object>>(
                    Expression.New(codeContainerListViewModelConstructor, // new CodeContainerListViewModel( 
                                                                          // new CodeContainerStorageManagerFactory(serviceProvider).Create()
                        Expression.Call(
                            Expression.New(codeContainerStorageManagerFactoryConstructor, serviceProviderParameter),
                            codeContainerStorageManagerFactoryCreate),

                        // new CodeContainerAccessManager(serviceProvider)
                        Expression.New(codeContainerAccessManagerConstructor, newCodeContainerAccessManagerConstructorParams),

                        // new RemovalPromptProvider(serviceProvider)
                        Expression.New(removalPromptProviderConstructor, serviceProviderParameter),

                        // new CodeContainerIconMonikerProvider(serviceProvider)
                        Expression.New(codeContainerIconMonikerProviderConstuctor, serviceProviderParameter),

                        // TimeCategoryProvider.Instance
                        Expression.Property(null, timeCategoryProviderInstanceProperty),

                        // new CodeContainerTelemetryLogger()
                        Expression.New(codeContainerTelemetryLoggerConstuctor, newCodeContainerTelemetryLoggerParams)
                    ),
                    serviceProviderParameter,
                    asyncServiceProviderParameter
                ).Compile();
            }
        }

        public static UserControl GettingStartedControl(object parent)
        {
            return GetTemplateChild<UserControl>(parent, "GettingStartedControl");
        }

        public static Expander NewsExpander(object parent)
        {
            return GetTemplateChild<Expander>(parent, "NewsExpander");
        }

        public static UserControl CodeContainerProvidersListView(object parent)
        {
            return GetTemplateChild<UserControl>(parent, "CodeContainerProvidersListView");
        }

        public static UserControl NewProjectsListView(object parent)
        {
            return GetTemplateChild<UserControl>(parent, "NewProjectsListView");
        }

        public static Panel MRUPanel(object parent)
        {
            return GetTemplateChild<Panel>(parent, "MRUPanel");
        }

        public static Grid MainPanel(object parent)
        {
            return GetTemplateChild<Grid>(parent, "MainPanel");
        }

        private static T GetTemplateChild<T>(object parent, string name) where T : class
        {
            return Start_GetTemplateChildInternal((FrameworkElement)parent, name) as T;
        }

        public static void SetupDataContexts(object parent)
        {
            Shell15.Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await TaskScheduler.Default;

                    var asyncProvider = Package.GetGlobalService(typeof(SAsyncServiceProvider));
                    var serviceProvider = Ioc.Instance.Resolve<IServiceProvider>();
                    var codeContainerListViewModel = CodeContainerListViewModel_New(serviceProvider, asyncProvider);

                    await Shell15.Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    var newProjectsListView = NewProjectsListView(parent);
                    if (newProjectsListView != null)
                    {
                        object newProjectsListViewDataContext = null;
                        if (asyncProvider != null)
                        {
                            newProjectsListViewDataContext = NewProjectsListViewModel_New(asyncProvider);
                            await NewProjectsListViewModel_SetMoreTemplatesTextRemoteSettingIfAvailableAsync(
                                newProjectsListViewDataContext);
                            await NewProjectsListViewModel_LoadRecentTemplatesAsync(newProjectsListViewDataContext);
                        }
                        newProjectsListView.DataContext = newProjectsListViewDataContext;
                    }

                    var codeContainerProvidersListView = CodeContainerProvidersListView(parent);
                    if (codeContainerProvidersListView != null)
                    {
                        var codeContainerProviderService =
                            serviceProvider.GetService(typeof(SVsCodeContainerProviderService));
                        codeContainerProvidersListView.DataContext = codeContainerProviderService != null
                            ? CodeContainerProviderService_CodeContainerProviders(codeContainerProviderService)
                            : null;
                    }

                    var mruPanel = MRUPanel(parent);
                    if (mruPanel != null)
                    {
                        mruPanel.DataContext = codeContainerListViewModel;
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        ActivityLog.LogError("StartPage", e.ToString());
                    }
                    catch
                    {
                    }
                }
            });
        }


        #region Service GUIDs

        // for whatever reason using the interface contained in Microsoft.VisualStudio.Shell.15.0 leads to 
        // error CS0234: The type or namespace name 'SAsyncServiceProvider' does not exist in the namespace 'Microsoft.VisualStudio.Shell.Interop' (are you missing an assembly reference?)
        [Guid("944774C9-7422-4E87-B01C-189182C779A6")]
        private interface SAsyncServiceProvider
        {
        }

        #endregion

    }
}