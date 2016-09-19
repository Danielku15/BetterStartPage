namespace BetterStartPage.Control
{
    internal interface IIdeAccess
    {
        void OpenProject(string name);
        bool ShowDeleteConfirmation(string text);
        void OpenFile(string name);
        bool ShowProjectRenameDialog(string name, out string newName);
        bool ShowMissingFileDialog(string fullName);
    }
}
