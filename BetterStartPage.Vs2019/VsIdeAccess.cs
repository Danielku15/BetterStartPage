using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using BetterStartPage.Control.ViewModel;
using BetterStartPage.View;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace BetterStartPage
{
    public class VsIdeAccess : IIdeAccess
    {
        private readonly DTE2 _ide;

        public VsIdeAccess(DTE2 ide)
        {
            _ide = ide;
        }

        public void OpenProject(string name)
        {
            if (_ide != null)
            {
                if (QuickStartWindow.Instance != null)
                {
                    QuickStartWindow.Instance.CompleteWorkflow(() => { InternalOpenProject(name); });
                }
                else
                {
                    InternalOpenProject(name);
                }
            }
        }

        private void InternalOpenProject(string name)
        {
            var solution = (IVsSolution)Package.GetGlobalService(typeof(IVsSolution));
            if (name.EndsWith(".sln"))
            {
                solution.OpenSolutionFile(0, name);
            }
            else
            {
                var ppProject = IntPtr.Zero;
                var iidProject = Guid.Empty;
                var rguidProjectType = Guid.Empty;
                solution.CreateProject(ref rguidProjectType, name, string.Empty, string.Empty, 2U,
                    ref iidProject, out ppProject);
                if (ppProject != IntPtr.Zero)
                {
                    Marshal.Release(ppProject);
                }

                // we manually need to add the project to the MRU list
                AddToProjectMRUList(name);
            }
        }

        #region ProjectMRU

        private IVsUIDataSource _projectMruList;
        private static readonly Guid MruListDataSourceFactoryGuid = new Guid("9099ad98-3136-4aca-a9ac-7eeeaee51dca");
        public IVsUIDataSource ProjectMRUList
        {
            get
            {
                if (_projectMruList == null)
                {
                    var dataSourceFactory = (IVsDataSourceFactory)Package.GetGlobalService(typeof(SVsDataSourceFactory));
                    var guid = MruListDataSourceFactoryGuid;
                    dataSourceFactory.GetDataSource(ref guid, 1U, out _projectMruList);
                }
                return _projectMruList;
            }
        }

        private void AddToProjectMRUList(string fullPath)
        {
            ProjectMRUList.Invoke(MruListDataSourceSchema.AddCommandName, fullPath, out _);
        }

        #endregion

        public bool ShowDeleteConfirmation(string text)
        {
            return MessageBox.Show(text, "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.No) == MessageBoxResult.Yes;
        }

        public void OpenFile(string name)
        {
            if (_ide != null)
            {
                if (Utilities.IsHttp(name) || Directory.Exists(name))
                {
                    Process.Start(name);
                }
                else
                {
                    _ide.ExecuteCommand("File.OpenFile", $"\"{name}\"");
                }
            }
        }

        public bool ShowProjectRenameDialog(string name, out string newName)
        {
            newName = null;
            var vm = new ProjectRenameViewModel(name);
            var wnd = new ProjectRenameWindow { DataContext = vm };
            if (wnd.ShowDialog().GetValueOrDefault())
            {
                newName = vm.ProjectName;
                return true;
            }
            return false;
        }

        public bool ShowGroupRenameDialog(string name, out string newName)
        {
            newName = null;
            var vm = new ProjectRenameViewModel(name)
            {
                Title = "Rename Group"
            };
            var wnd = new ProjectRenameWindow { DataContext = vm };
            if (wnd.ShowDialog().GetValueOrDefault())
            {
                newName = vm.ProjectName;
                return true;
            }
            return false;
        }

        public bool ShowNewGroupDialog(out string name)
        {
            var vm = new ProjectRenameViewModel
            {
                Title = "New Group",
                ButtonTitle = "Create"
            };
            var wnd = new ProjectRenameWindow { DataContext = vm };
            if (wnd.ShowDialog().GetValueOrDefault())
            {
                name = vm.ProjectName;
                return true;
            }
            name = null;
            return false;
        }

        public bool ShowMissingFileDialog(string fullName)
        {
            return MessageBox.Show(
                $"The File '{fullName}' could not be found on the disk, do you want to remove it from the StartPage?",
                "File not found",
                MessageBoxButton.YesNo, MessageBoxImage.Error
            ) == MessageBoxResult.Yes;
        }

        public string ShowExportConfigurationDialog()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "BetterStartPage Configuration (*.bspc)|*.bspc";
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                return dialog.FileName;
            }
            return null;
        }

        public string ShowImportConfigurationDialog()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "BetterStartPage Configuration (*.bspc)|*.bspc";
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                return dialog.FileName;
            }
            return null;
        }

        public void ShowExportResultDialog(string file, Exception error)
        {
            if (error != null)
            {
                MessageBox.Show(
                    error.Message,
                    $"Could not export configuration to '{file}'",
                    MessageBoxButton.OK, MessageBoxImage.Error
                );
            }
            else
            {
                MessageBox.Show(
                    $"The configuration was successfully exported to '{file}'",
                    $"Configuration successfully exported",
                    MessageBoxButton.OK, MessageBoxImage.Information
                );
            }
        }

        public void ShowImportResultDialog(string file, Exception error)
        {
            if (error != null)
            {
                MessageBox.Show(
                    error.Message,
                    $"Could not load configuration from '{file}'",
                    MessageBoxButton.OK, MessageBoxImage.Error
                );
            }
            else
            {
                MessageBox.Show(
                    $"The configuration was successfully imported from '{file}'",
                    $"Configuration successfully imported",
                    MessageBoxButton.OK, MessageBoxImage.Information
                );
            }
        }
    }
}