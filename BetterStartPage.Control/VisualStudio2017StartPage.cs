extern alias Shell15;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI.Packages.StartPage;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Expression = System.Linq.Expressions.Expression;

namespace BetterStartPage.Control
{
    extern alias Shell15;
    /// <summary>
    /// This class contains special logic for interacting with the VS2017 start page. 
    /// </summary>
    public static class VisualStudio2017StartPage
    {
        private static readonly Func<object, string, object> Start_GetTemplateChildInternal;
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
            try
            {
                // Start_GetTemplateChildInternal
                {
                    var startType = typeof(Start);
                    var frameworkElementParameter = Expression.Parameter(typeof(object), "frameworkElement");
                    var nameParameter = Expression.Parameter(typeof(string), "name");
                    var getTemplateChildMethodGeneric = typeof(Start)
                            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
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
            }
            catch (Exception e)
            {
                ActivityLog.LogError("StartPage", "Failed to load StartPage internals: " + e.Message);
            }
        }

        public static UserControl GettingStartedControl(FrameworkElement parent)
        {
            if (parent is Start start)
            {
                return start.GettingStartedControl;
            }
            return GetTemplateChild<UserControl>(parent, "GettingStartedControl");
        }

        public static Panel MRUPanel(FrameworkElement parent)
        {
            if (parent is Start start)
            {
                return start.MRUPanel;
            }
            return GetTemplateChild<Panel>(parent, "MRUPanel");
        }

        public static Grid MainPanel(FrameworkElement parent)
        {
            return GetTemplateChild<Grid>(parent, "MainPanel");
        }

        private static T GetTemplateChild<T>(FrameworkElement parent, string name) where T : class
        {
            return Start_GetTemplateChildInternal(parent, name) as T;
        }

        public static async Task<FrameworkElement> CreateOriginalStartPage()
        {
            // create the original StartPageToolWindow
            StartPageToolWindow startWindow = new StartPageToolWindow();
            startWindow.LoadAndSet("Microsoft.VisualStudio.Shell.UI.Internal;component/packages/startpage/controls/start.xaml");

            // search for the start component 
            var result = FindStart(startWindow.Content);
            if (result == null)
            {
                return null;
            }
            var parent = result.Item1;
            var start = result.Item2;

            // The data context is initialized delayed in an async call. 
            // we need to wait for the context to become available before we remove the start control from the toolwindow. 
            if (start.MRUPanel.DataContext == null)
            {
                using (var waitForContext = new ManualResetEvent(false))
                {
                    void SetHandle(object sender, DependencyPropertyChangedEventArgs e)
                    {
                        waitForContext.Set();
                    }

                    start.MRUPanel.DataContextChanged += SetHandle;
                    var sw = Stopwatch.StartNew();
                    var timeout = await WaitHandleToTask(waitForContext, 3000);
                    start.MRUPanel.DataContextChanged -= SetHandle;
                    sw.Stop();

                    if (timeout)
                    {
                        ActivityLog.LogWarning("StartPage", "Data Context did not become available in 3000ms");
                        // NOTE: If this happens more often we could theoretically attempt to 
                        // set the internal StartPageToolWindow.ContentHost 
                        // this way the delayed datacontext initialization will still be able to access the 
                        // startpage even though we detach it below. 
                    }
                    else
                    {
                        ActivityLog.LogInformation("StartPage", "Data Context became available in " + sw.ElapsedMilliseconds + "ms");
                    }
                }
            }

            // We need to detach the start control and only return this one. 
            // the StartPageToolWindow would try to load again the "BetterStartPage"
            // causing a endless-loop and stackoverflow. 
            DetachStart(parent, start);

            return start;
        }

        public static System.Threading.Tasks.Task<bool> WaitHandleToTask(WaitHandle handle, int timeout)
        {
            if (handle == null) throw new ArgumentNullException("handle");

            var tcs = new TaskCompletionSource<bool>();
            RegisteredWaitHandle shared = null;
            RegisteredWaitHandle produced = ThreadPool.RegisterWaitForSingleObject(
                handle,
                (state, timedOut) =>
                {
                    tcs.SetResult(timedOut);

                    while (true)
                    {
                        RegisteredWaitHandle consumed = Interlocked.CompareExchange(ref shared, null, null);
                        if (consumed != null)
                        {
                            consumed.Unregister(null);
                            break;
                        }
                    }
                },
                state: null,
                millisecondsTimeOutInterval: timeout,
                executeOnlyOnce: true);

            // Publish the RegisteredWaitHandle so that the callback can see it.
            Interlocked.CompareExchange(ref shared, produced, null);

            return tcs.Task;
        }


        private static void DetachStart(FrameworkElement parent, Start start)
        {
            if (parent == null)
            {
                return;
            }

            if (parent is Decorator decorator)
            {
                decorator.Child = null;
            }
            else if (parent is ContentControl content)
            {
                content.Content = null;
            }
            else if (parent is Panel panel)
            {
                panel.Children.Remove(start);
            }
        }

        private static Tuple<FrameworkElement, Start> FindStart(object element)
        {
            if (element == null)
            {
                return null;
            }

            if (element is Start start)
            {
                return Tuple.Create<FrameworkElement, Start>(null, start);
            }

            if (element is Decorator decorator)
            {
                if (decorator.Child is Start ds)
                {
                    return Tuple.Create<FrameworkElement, Start>(decorator, ds);
                }

                return FindStart(decorator.Child);
            }

            if (element is ContentControl content)
            {
                if (content.Content is Start cs)
                {
                    content.Content = null;
                    return Tuple.Create<FrameworkElement, Start>(content, cs);
                }

                return FindStart(content.Content);
            }

            if (element is Panel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is Start ps)
                    {
                        panel.Children.Remove(ps);
                        return Tuple.Create<FrameworkElement, Start>(panel, ps);
                    }
                    return FindStart(child);
                }
            }

            return null;
        }
    }
}