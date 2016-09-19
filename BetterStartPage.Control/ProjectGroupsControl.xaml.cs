using System;
using System.Windows;
using BetterStartPage.Control.Settings;
using BetterStartPage.Control.ViewModel;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace BetterStartPage.Control
{
    public partial class ProjectGroupsControl
    {
        public ProjectGroupsControl()
            : this(null)
        {
        }

        public ProjectGroupsControl(DTE2 dte)
        {
            InitializeComponent();

            var ideAccess = new VsIdeAccess(dte);
            ISettingsProvider settingsProvider;
            if (dte == null)
            {
                settingsProvider = new DummySettingsProvider();
            }
            else
            {
                settingsProvider = new VsSettingsProvider(new ServiceProvider((IServiceProvider)dte));
            }

            try
            {
                var viewModel = new ProjectGroupsViewModel(ideAccess, settingsProvider);
                DataContext = viewModel;
            }
            catch (Exception)
            {
                var result = MessageBox.Show(
                    "Could not load existing project groups from settings. Do you want to reset the invalid settings?",
                    "Group Loading failed", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    settingsProvider.Reset();
                }
            }
        }
    }
}
