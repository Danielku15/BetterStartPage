namespace BetterStartPage.Control.ViewModel
{
    class ProjectRenameViewModel : ViewModelBase
    {
        private string _projectName;

        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                if (value == _projectName) return;
                _projectName = value;
                OnPropertyChanged();
            }
        }

        public ProjectRenameViewModel()
        {
        }

        public ProjectRenameViewModel(string projectName)
        {
            _projectName = projectName;
        }
    }
}
