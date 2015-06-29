using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using BetterStartPage.Control.ViewModel;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BetterStartPage.Control
{
    class VsIdeAccess : IIdeAccess
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
                var solution = (IVsSolution)Package.GetGlobalService(typeof(IVsSolution));
                if (name.EndsWith(".sln"))
                {
                    solution.OpenSolutionFile(0, name);
                }
                else
                {
                    IntPtr ppProject = IntPtr.Zero;
                    Guid iidProject = VSConstants.IID_IUnknown;
                    Guid rguidProjectType = Guid.Empty;
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
            object vaOut;
            ProjectMRUList.Invoke(MruListDataSourceSchema.AddCommandName, fullPath, out vaOut);
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
                    _ide.ExecuteCommand("File.OpenFile", String.Format("\"{0}\"", name));
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

        public bool ShowMissingFileDialog(string fullName)
        {
            return MessageBox.Show(
                string.Format("The File '{0}' could not be found on the disk, do you want to remove it from the StartPage?", fullName),
                "File not found",
                MessageBoxButton.YesNo, MessageBoxImage.Error
            ) == MessageBoxResult.Yes;
        }
    }
}