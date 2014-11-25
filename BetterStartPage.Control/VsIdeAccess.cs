using System;
using System.IO;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Process = System.Diagnostics.Process;

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
                _ide.ExecuteCommand("File.OpenProject", String.Format("\"{0}\"", name));
            }
        }

        public bool ShowDeleteConfirmation(string text)
        {
            return MessageBox.Show(text, "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.No) == MessageBoxResult.Yes;
        }

        public void OpenFile(string name)
        {
            if (_ide != null)
            {
                if (Directory.Exists(name))
                {
                    Process.Start(name);
                }
                else
                {
                    _ide.ExecuteCommand("File.OpenFile", String.Format("\"{0}\"", name));
                }
            }
        }
    }
}