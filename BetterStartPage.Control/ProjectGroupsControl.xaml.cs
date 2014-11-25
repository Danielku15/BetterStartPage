using System.Windows;
using BetterStartPage.Control.Settings;
using BetterStartPage.Control.ViewModel;
using EnvDTE80;

namespace BetterStartPage.Control
{
    public partial class ProjectGroupsControl
    {
        private DTE2 _ide;

        private ISettingsProvider _settingsProvider;
        private ISettingsProvider SettingsProvider
        {
            get
            {
                if (_settingsProvider == null)
                {
                    if (_ide == null)
                    {
                        _settingsProvider = new DummySettingsProvider();
                    }
                    else
                    {
                        _settingsProvider = new VsSettingsProvider(Utilities.GetServiceProvider(_ide));
                    }
                }
                return _settingsProvider;
            }
        }


        private IIdeAccess _ideAccess;
        private IIdeAccess IdeAccess
        {
            get
            {
                if (_ideAccess == null)
                {
                    _ideAccess = new VsIdeAccess(_ide);
                }
                return _ideAccess;
            }
        }

        public ProjectGroupsControl()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _ide = Utilities.GetDTE(DataContext);
            var viewModel = new ProjectGroupsViewModel(IdeAccess);
            DataContext = viewModel;
            if (!viewModel.Setup(SettingsProvider))
            {
               var result = MessageBox.Show(
                    "Could not load existing project groups from settings. Do you want to reset the invalid settings?",
                    "Group Loading failed", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    SettingsProvider.Reset();
                }
            }
        }
    }
}
