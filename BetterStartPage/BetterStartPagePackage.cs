using System.Runtime.InteropServices;
using BetterStartPage.Control;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BetterStartPage
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] 
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids.NoSolution)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [Guid(PackageGuidString)]
    public sealed class BetterStartPagePackage : Package, IVsShellPropertyEvents
    {
        public const string PackageGuidString = "c022287d-6bb7-4e26-a08c-e5d46cd67d93";

        protected override void Initialize()
        {
            StartPageBootstrapper.Initialize(this);
            ProjectGroupsMenu.Initialize(this);
            base.Initialize();
        }

        public int OnShellPropertyChange(int propid, object var)
        {
            throw new System.NotImplementedException();
        }
    }
}