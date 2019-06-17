using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BetterStartPage.Control.Settings;
using BetterStartPage.Settings;
using BetterStartPage.View;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.Win32;

namespace BetterStartPage.ViewModel
{
    internal class ProjectGroupsViewModel : ViewModelBase
    {
        private readonly IIdeAccess _ideAccess;
        private int _groupColumns;
        private ObservableCollection<ProjectGroup> _groups;
        private bool _isEditMode;
        private readonly ISettingsProvider _settingsProvider;
        private int _projectColumns;

        public int GroupColumns
        {
            get { return _groupColumns; }
            set
            {
                if (value == _groupColumns) return;
                _groupColumns = Math.Max(1, value);
                ((RelayCommand)DecreaseGroupColumnsCommand).RaiseCanExecuteChanged();
                OnPropertyChanged();
            }
        }

        public int ProjectColumns
        {
            get { return _projectColumns; }
            set
            {
                if (value == _projectColumns) return;
                _projectColumns = Math.Max(0, value);
                ((RelayCommand)DecreaseProjectColumnsCommand).RaiseCanExecuteChanged();
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
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public bool IsEmpty
        {
            get { return Groups.Count == 0; }
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

        public ICommand OpenProjectCommand { get; }
        public ICommand OpenDirectoryCommand { get; }
        public ICommand OpenAllFilesCommand { get; }

        public ICommand AddGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand MoveGroupUpCommand { get; }
        public ICommand MoveGroupDownCommand { get; }
        public ICommand AddProjectsToGroupCommand { get; }

        public ICommand AddProjectsCommand { get; }
        public ICommand RenameProjectCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand MoveProjectUpCommand { get; }
        public ICommand MoveProjectDownCommand { get; }

        public ICommand IncreaseGroupColumnsCommand { get; }
        public ICommand DecreaseGroupColumnsCommand { get; }
        public ICommand IncreaseProjectColumnsCommand { get; }
        public ICommand DecreaseProjectColumnsCommand { get; }

        public ICommand ExportConfigurationCommand { get; }
        public ICommand ImportConfigurationCommand { get; }

        public ProjectGroupsViewModel(IIdeAccess ideAccess, ISettingsProvider settingsProvider)
        {
            _ideAccess = ideAccess;
            _settingsProvider = settingsProvider;

            OpenProjectCommand = new RelayCommand<Project>(OpenProject);
            OpenDirectoryCommand = new RelayCommand<Project>(OpenDirectory);
            OpenAllFilesCommand = new RelayCommand<ProjectGroup>(OpenAllFiles);
            AddGroupCommand = new RelayCommand(NewGroup);
            DeleteGroupCommand = new RelayCommand<ProjectGroup>(DeleteGroup);
            MoveGroupUpCommand = new RelayCommand<ProjectGroup>(MoveGroupUp);
            MoveGroupDownCommand = new RelayCommand<ProjectGroup>(MoveGroupDown);
            AddProjectsToGroupCommand = new RelayCommand<ProjectGroup>(AddProjectsToGroup);

            AddProjectsCommand = new RelayCommand<FilesDroppedEventArgs>(AddProjects);
            RenameProjectCommand = new RelayCommand<Project>(RenameProject);
            DeleteProjectCommand = new RelayCommand<Project>(DeleteProject);
            MoveProjectUpCommand = new RelayCommand<Project>(MoveProjectUp);
            MoveProjectDownCommand = new RelayCommand<Project>(MoveProjectDown);

            IncreaseGroupColumnsCommand = new RelayCommand(IncreaseGroupColumns);
            DecreaseGroupColumnsCommand = new RelayCommand(DecreaseGroupColumns, () => GroupColumns > 1);

            IncreaseProjectColumnsCommand = new RelayCommand(IncreaseProjectColumns);
            DecreaseProjectColumnsCommand = new RelayCommand(DecreaseProjectColumns, () => ProjectColumns > 0);

            ExportConfigurationCommand = new RelayCommand(ExportConfiguration);
            ImportConfigurationCommand = new RelayCommand(ImportConfiguration);

            Setup();
        }

        private void ImportConfiguration()
        {
            var file = _ideAccess.ShowImportConfigurationDialog();
            if (!string.IsNullOrEmpty(file))
            {
                try
                {
                    var model = SettingsExportViewModel.Deserialize(File.ReadAllBytes(file));
                    Groups = new ObservableCollection<ProjectGroup>(model.ProjectGroups ?? new ProjectGroup[0]);
                    Groups.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(IsEmpty));
                    GroupColumns = model.GroupColumns;
                    ProjectColumns = model.ProjectColumns;

                    PersistSettings();

                    _ideAccess.ShowImportResultDialog(file);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Importing settings failed", e);
                    _ideAccess.ShowImportResultDialog(file, e);
                }
            }
        }

        private void ExportConfiguration()
        {
            var file = _ideAccess.ShowExportConfigurationDialog();
            if (!string.IsNullOrEmpty(file))
            {
                var model = new SettingsExportViewModel
                {
                    ProjectGroups = Groups.ToArray(),
                    GroupColumns = GroupColumns,
                    ProjectColumns = ProjectColumns
                };

                try
                {
                    File.WriteAllBytes(file, SettingsExportViewModel.Serialize(model));
                    _ideAccess.ShowExportResultDialog(file);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exporting settings failed", e);
                    _ideAccess.ShowExportResultDialog(file, e);
                }
            }
        }

        private void DecreaseGroupColumns()
        {
            GroupColumns--;
        }

        private void IncreaseGroupColumns()
        {
            GroupColumns++;
        }

        private void DecreaseProjectColumns()
        {
            ProjectColumns--;
        }

        private void IncreaseProjectColumns()
        {
            ProjectColumns++;
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

        private void OpenDirectory(Project project)
        {
            _ideAccess.OpenFile(project.DirectoryName);
        }

        #region Projects

        private void AddProjects(FilesDroppedEventArgs args)
        {
            var group = (ProjectGroup)args.DropTarget;
            var files = (string[])args.DropData;

            AddProjects(group, files);
        }

        private void AddProjects(ProjectGroup group, string[] files)
        {
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
                $"Are you sure you want to delete '{project.FullName}'?"))
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
        }

        private void AddProjectsToGroup(ProjectGroup group)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "All Project and Solution Files (*.sln, *.csproj, *.vcxproj, *.vbproj)|*.sln;*.csproj;*.vcxproj;*.vbproj|All Files (*.*)|*.*";
            dlg.Multiselect = true;
            if (dlg.ShowDialog(Application.Current.MainWindow).GetValueOrDefault())
            {
                AddProjects(group, dlg.FileNames);
            }
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
                $"Are you sure you want to delete '{group.Title}'?"))
            {
                _groups.Remove(group);
            }
        }

        #endregion

        private void MoveGroup<T>(IList<T> list, int indexToMove, bool up)
        {
            if (list.Count == 1) return;

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

        private void Setup()
        {
            var storedGroups = _settingsProvider.ReadBytes("StoredGroups");
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
                    Debug.WriteLine("Loading Start Page settings failed {0}", e);
                    throw;
                }
            }

            Groups = new ObservableCollection<ProjectGroup>(groups ?? new ProjectGroup[0]);
            Groups.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(IsEmpty));
            GroupColumns = _settingsProvider.ReadInt32("GroupColumns", 1);
            ProjectColumns = _settingsProvider.ReadInt32("ProjectColumns");
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
            _settingsProvider.WriteInt32("ProjectColumns", ProjectColumns);
        }

        #endregion
    }
}
