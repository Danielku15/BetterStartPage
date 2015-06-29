using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using BetterStartPage.Control.Settings;

namespace BetterStartPage.Control.ViewModel
{
    class ProjectGroupsViewModel : ViewModelBase
    {
        private readonly IIdeAccess _ideAccess;
        private int _groupColumns;
        private ObservableCollection<ProjectGroup> _groups;
        private int _groupRows;
        private bool _isEditMode;
        private ISettingsProvider _settingsProvider;

        public int GroupColumns
        {
            get { return _groupColumns; }
            private set
            {
                if (value == _groupColumns) return;
                _groupColumns = Math.Max(1, value);
                ((RelayCommand)DecreaseGroupColumnsCommand).RaiseCanExecuteChanged();
                UpdateGroupRows();
                OnPropertyChanged();
            }
        }

        public int GroupRows
        {
            get { return _groupRows; }
            set
            {
                _groupRows = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ProjectGroup> Groups
        {
            get { return _groups; }
            private set
            {
                if (Equals(value, _groups)) return;
                _groups = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditMode
        {
            get { return _isEditMode; }
            set
            {
                if (value.Equals(_isEditMode)) return;
                _isEditMode = value;
                OnPropertyChanged();
                if (!IsEditMode)
                {
                    PersistSettings();
                }
            }
        }

        public ICommand OpenProjectCommand { get; private set; }
        public ICommand OpenAllFilesCommand { get; private set; }

        public ICommand AddGroupCommand { get; private set; }
        public ICommand DeleteGroupCommand { get; private set; }
        public ICommand MoveGroupUpCommand { get; private set; }
        public ICommand MoveGroupDownCommand { get; private set; }

        public ICommand AddProjectsCommand { get; private set; }
        public ICommand RenameProjectCommand { get; private set; }
        public ICommand DeleteProjectCommand { get; private set; }
        public ICommand MoveProjectUpCommand { get; private set; }
        public ICommand MoveProjectDownCommand { get; private set; }

        public ICommand IncreaseGroupColumnsCommand { get; private set; }
        public ICommand DecreaseGroupColumnsCommand { get; private set; }

        public ProjectGroupsViewModel(IIdeAccess ideAccess)
        {
            _ideAccess = ideAccess;
            OpenProjectCommand = new RelayCommand<Project>(OpenProject);
            OpenAllFilesCommand = new RelayCommand<ProjectGroup>(OpenAllFiles);
            AddGroupCommand = new RelayCommand(NewGroup);
            DeleteGroupCommand = new RelayCommand<ProjectGroup>(DeleteGroup);
            MoveGroupUpCommand = new RelayCommand<ProjectGroup>(MoveGroupUp);
            MoveGroupDownCommand = new RelayCommand<ProjectGroup>(MoveGroupDown);

            AddProjectsCommand = new RelayCommand<FilesDroppedEventArgs>(AddProjects);
            RenameProjectCommand = new RelayCommand<Project>(RenameProject);
            DeleteProjectCommand = new RelayCommand<Project>(DeleteProject);
            MoveProjectUpCommand = new RelayCommand<Project>(MoveProjectUp);
            MoveProjectDownCommand = new RelayCommand<Project>(MoveProjectDown);

            IncreaseGroupColumnsCommand = new RelayCommand(IncreaseGroupColumns);
            DecreaseGroupColumnsCommand = new RelayCommand(DecreaseGroupColumns, () => GroupColumns > 1);
        }

        private void DecreaseGroupColumns()
        {
            GroupColumns--;
        }

        private void IncreaseGroupColumns()
        {
            GroupColumns++;
        }

        private void OpenAllFiles(ProjectGroup group)
        {
            foreach (var project in group.Projects.Where(p => p.IsNormalFile))
            {
                OpenProject(project);
            }
        }

        private void OpenProject(Project project)
        {
            if (!File.Exists(project.FullName) && !Directory.Exists(project.FullName))
            {
                var shouldRemove = _ideAccess.ShowMissingFileDialog(project.FullName);
                if (shouldRemove)
                {
                    InternalDeleteProject(project);
                }
            }
            else if (project.IsNormalFile)
            {
                _ideAccess.OpenFile(project.FullName);
            }
            else
            {
                _ideAccess.OpenProject(project.FullName);
            }
        }

        private void UpdateGroupRows()
        {
            GroupRows = (int)Math.Ceiling(Groups.Count / (double)GroupColumns);
        }

        #region Projects

        private void AddProjects(FilesDroppedEventArgs args)
        {
            var group = (ProjectGroup)args.DropTarget;
            var files = (string[])args.DropData;

            var existingProjects = new HashSet<string>(group.Projects.Select(p => p.FullName), StringComparer.InvariantCultureIgnoreCase);

            var projects = files
                .Where(f => !existingProjects.Contains(f) && IsSupportedUri(f))
                .Select(f => new Project(f));

            var groupProjects = (ObservableCollection<Project>)group.Projects;
            foreach (var project in projects)
            {
                groupProjects.Add(project);
            }
        }

        private bool IsSupportedUri(string s)
        {
            return File.Exists(s) || Directory.Exists(s) || Utilities.IsHttp(s);
        }

        private void MoveProjectDown(Project project)
        {
            MoveProject(project, false);
        }

        private void MoveProjectUp(Project project)
        {
            MoveProject(project, true);
        }

        private ProjectGroup GetGroupOfProject(Project project)
        {
            return _groups.FirstOrDefault(g => g.Projects.Contains(project));
        }

        private void MoveProject(Project project, bool up)
        {
            ProjectGroup group = GetGroupOfProject(project);
            if (group == null) return;
            int index = ((IList<Project>)group.Projects).IndexOf(project);
            if (index == -1) return;

            MoveGroup((IList<Project>)group.Projects, index, up);
        }

        private void RenameProject(Project project)
        {
            ProjectGroup group = GetGroupOfProject(project);
            if (group == null) return;

            string newName;
            if (_ideAccess.ShowProjectRenameDialog(project.Name, out newName))
            {
                project.CustomName = newName;
            }
        }

        private void DeleteProject(Project project)
        {
            if (_ideAccess.ShowDeleteConfirmation(
                string.Format("Are you sure you want to delete '{0}'?", project.FullName)))
            {
                InternalDeleteProject(project);
            }
        }

        private void InternalDeleteProject(Project project)
        {
            ProjectGroup group = GetGroupOfProject(project);
            if (group == null) return;
            ((IList<Project>)group.Projects).Remove(project);
            if (!IsEditMode)
            {
                PersistSettings();
            }
        }

        #endregion

        #region Groups

        private void NewGroup()
        {
            Groups.Add(new ProjectGroup
            {
                Title = "New Group",
                Projects = new ObservableCollection<Project>()
            });
            UpdateGroupRows();
        }

        private void MoveGroupDown(ProjectGroup group)
        {
            int index = _groups.IndexOf(group);
            if (index == -1) return;
            MoveGroup(index, false);
        }

        private void MoveGroupUp(ProjectGroup group)
        {
            int index = _groups.IndexOf(group);
            if (index == -1) return;
            MoveGroup(index, true);
        }

        private void MoveGroup(int indexToMove, bool up)
        {
            MoveGroup(_groups, indexToMove, up);
        }

        private void DeleteGroup(ProjectGroup group)
        {
            if (_ideAccess.ShowDeleteConfirmation(
                string.Format("Are you sure you want to delete '{0}'?", group.Title)))
            {
                _groups.Remove(group);
            }
        }

        #endregion

        private void MoveGroup<T>(IList<T> list, int indexToMove, bool up)
        {
            if (up)
            {
                if (indexToMove == 0) return;

                var old = list[indexToMove - 1];
                list[indexToMove - 1] = list[indexToMove];
                list[indexToMove] = old;
            }
            else
            {
                if (indexToMove == _groups.Count - 1) return;

                var old = list[indexToMove + 1];
                list[indexToMove + 1] = list[indexToMove];
                list[indexToMove] = old;
            }
        }



        #region Loading / Saving

        public bool Setup(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            var successful = true;
            var storedGroups = settingsProvider.ReadBytes("StoredGroups");
            ProjectGroup[] groups = null;
            if (storedGroups != null && storedGroups.Length > 0)
            {
                try
                {
                    groups = ProjectGroup.Deserialize(storedGroups);

                    // ensure a observablecollection behind the scenes
                    foreach (var group in groups)
                    {
                        group.Projects = new ObservableCollection<Project>(group.Projects);
                    }

                }
                catch (Exception e)
                {
                    successful = false;
                    Debug.WriteLine("Loading Start Page settings failed {0}", e);
                }
            }

            Groups = new ObservableCollection<ProjectGroup>(groups ?? new ProjectGroup[0]);
            GroupColumns = settingsProvider.ReadInt32("GroupColumns", 1);

            return successful;
        }

        private void PersistSettings()
        {
            if (_settingsProvider == null) return;

            try
            {
                _settingsProvider.WriteBytes("StoredGroups", ProjectGroup.Serialize(Groups.ToArray()));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Loading Start Page settings failed {0}", e);
            }

            _settingsProvider.WriteInt32("GroupColumns", GroupColumns);
        }

        #endregion
    }
}
