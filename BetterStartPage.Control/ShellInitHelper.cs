using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BetterStartPage.Control
{
    /// <summary>
    /// This is a small helper which can be used to execute a certain action when the VS shell is initialized. 
    /// </summary>
    internal class ShellInitHelper : IVsShellPropertyEvents
    {
        private readonly IVsShell _shell;
        private readonly Action _init;
        private uint _cookie;

        public ShellInitHelper(IVsShell shell, Action init)
        {
            _shell = shell;
            _init = init;
        }

        public void Setup()
        {
            object isReady;
            ErrorHandler.ThrowOnFailure(_shell.GetProperty((int)__VSSPROPID4.VSSPROPID_ShellInitialized, out isReady));
            if ((bool) isReady)
            {
                _init();
            }
            else
            {
                ErrorHandler.ThrowOnFailure(_shell.AdviseShellPropertyChanges(this, out _cookie));
            }
        }

        public int OnShellPropertyChange(int propid, object var)
        {
            if (propid == (int)__VSSPROPID4.VSSPROPID_ShellInitialized && (bool)var)
            {
                _init();
                try
                {
                    if (_cookie != 0)
                    {
                        ErrorHandler.ThrowOnFailure(_shell.UnadviseShellPropertyChanges(_cookie));
                        _cookie = 0;
                    }
                }
                catch (Exception e)
                {
                    ActivityLog.LogError("StartPage", e.ToString());
                }
            }
            return 0;

        }
    }
}