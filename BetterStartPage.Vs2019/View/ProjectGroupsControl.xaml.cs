using System;
using System.Windows;
using BetterStartPage.Settings;
using BetterStartPage.ViewModel;

namespace BetterStartPage.View
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
