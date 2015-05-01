namespace BetterStartPage.Control
{
    interface IIdeAccess
    {
        void OpenProject(string name);
        bool ShowDeleteConfirmation(string text);
        void OpenFile(string name);
        bool ShowProjectRenameDialog(string name, out string newName);
    }
}
