namespace BetterStartPage.Control.ViewModel
{
    internal class ProjectRenameViewModel : ViewModelBase
    {
        private string _projectName;
        private string _title;
        private string _buttonTitle;

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

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public string ButtonTitle
        {
            get => _buttonTitle;
            set
            {
                if (value == _buttonTitle) return;
                _buttonTitle = value;
                OnPropertyChanged();
            }
        }

        public ProjectRenameViewModel()
        : this(string.Empty)
        {

        }

        public ProjectRenameViewModel(string projectName)
        {
            Title = "Rename Project";
            ButtonTitle = "Rename";
            _projectName = projectName;
        }
    }
}
