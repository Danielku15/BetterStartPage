using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BetterStartPage.Control.ViewModel;
using BetterStartPage.Settings;
using BetterStartPage.Vs2019;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;
using Task = System.Threading.Tasks.Task;

namespace BetterStartPage
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    public sealed class BetterStartPagePackage : AsyncPackage
    {
        private const string PackageGuidString = "5ef73784-ad19-4cae-ae6d-27a822e33a01";

        private const string GetToCodePackageGuidString = "D208A515-B37C-4F88-AC23-F3727FE307BD";

        private const string GetToCodeCmdSetGuidString = "7c57081e-4f31-4ebf-a96f-4769e1d688ec";
        private const int ShowStartWindowCommand = 288;


        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var dte = (DTE2)(await GetServiceAsync(typeof(DTE)));
            var solution = (IVsSolution)(await GetServiceAsync(typeof(IVsSolution)));

            var showStartWindowCommand = await LoadShowStartWindowCommandAsync(cancellationToken);

            Ioc.Instance.RegisterInstance(dte);
            Ioc.Instance.RegisterInstance(solution);

            Ioc.Instance.RegisterInstance<IServiceProvider>(this);
            Ioc.Instance.Register<IIdeAccess, VsIdeAccess>();
            Ioc.Instance.Register<ISettingsProvider, VsSettingsProvider>();
            Ioc.Instance.Register<ProjectGroupsViewModel>();

            try
            {
                var menuCommandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
                if (menuCommandService != null)
                {
                    ProjectGroupsMenu.Initialize(menuCommandService);
                }
                else
                {
                    ActivityLog.LogError("BetterStartPage", "Failed to Register Menu: menu command service not found");
                }
            }
            catch (Exception e)
            {
                ActivityLog.LogError("BetterStartPage", "Failed to Register Menu: " + e);
            }

            try
            {
                QuickStartWindow.DialogCreated += (s,e) => QuickStartWindow.Instance?.PatchDialog();
                QuickStartWindow.Instance?.PatchDialog();
                if(Application.Current?.MainWindow != null)
                {
                    showStartWindowCommand?.Invoke();
                }
            }
            catch (Exception e)
            {
                ActivityLog.LogError("BetterStartPage", "Failed to initialize QuickStartWindow: " + e);
            }
        }

        private async Task<MenuCommand> LoadShowStartWindowCommandAsync(CancellationToken cancellation)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);

            // Ensure GetToCode package is loaded
            var vsShell = (IVsShell)await GetServiceAsync(typeof(SVsShell));
            var getToCodePackageGuid = new Guid(GetToCodePackageGuidString);
            IVsPackage package;
            if (vsShell == null ||
                vsShell.IsPackageLoaded(ref getToCodePackageGuid, out package) != 0 &&
                vsShell.LoadPackage(ref getToCodePackageGuid, out package) != 0)
            {
                ActivityLog.LogError("BetterStartPage", "Failed to load GetToCode Package");
                return null;
            }

            // Load ShowStartWindowCommand from GetToCode package
            var getToCodePackage = (AsyncPackage)package;
            var serviceAsync = await getToCodePackage.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (serviceAsync == null)
            {
                ActivityLog.LogError("BetterStartPage", "Failed to load OleMenuCommandService");
                return null;
            }

            var showStartWindowCommand = serviceAsync.FindCommand(new CommandID(new Guid(GetToCodeCmdSetGuidString), ShowStartWindowCommand));
            if (showStartWindowCommand == null)
            {
                ActivityLog.LogError("BetterStartPage", "Failed to load Show Start Window Command");
                return null;
            }

            return showStartWindowCommand;
        }
    }
}
