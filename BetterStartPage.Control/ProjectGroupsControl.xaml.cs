using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
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
        {
            InitializeComponent();

            try
            {
                DataContext = Ioc.Instance.Resolve<ProjectGroupsViewModel>();
            }
            catch (Exception)
            {
                var result = MessageBox.Show(
                    "Could not load existing project groups from settings. Do you want to reset the invalid settings?",
                    "Group Loading failed", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    Ioc.Instance.Resolve<ISettingsProvider>().Reset();
                }
            }
        }
    }
}
