using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using BetterStartPage.Control.ViewModel;
using Microsoft.VisualStudio.Shell;

namespace BetterStartPage.Control
{
    internal sealed class ProjectGroupsMenuCommand : OleMenuCommand
    {
        private readonly Predicate<int> _matches;

        public ProjectGroupsMenuCommand(CommandID rootId, Predicate<int> matches, EventHandler invokeHandler, EventHandler beforeQueryStatusHandler)
            : base(invokeHandler, null /*changeHandler*/, beforeQueryStatusHandler, rootId)
        {
            if (matches == null)
            {
                throw new ArgumentNullException("matches");
            }

            _matches = matches;
        }

        public Project Project { get; set; }

        public override bool DynamicItemMatch(int cmdId)
        {
            if (_matches(cmdId))
            {
                MatchedCommandId = cmdId;
                return true;
            }

            MatchedCommandId = 0;
            return false;
        }
    }

    public sealed class ProjectGroupsMenu
    {
        public const int BetterStartPageMenuOpenFavouritePlaceholderCommand = 0x1100;
        public static readonly Guid CommandSet = new Guid("e6eebcb9-04ce-4b3f-b638-67985e03c664");

        private readonly Package _package;
        private readonly ProjectGroupsViewModel _viewModel;
        private List<Tuple<ProjectGroup, Project>> _flattened;

        public static ProjectGroupsMenu Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider => _package;

        private ProjectGroupsMenu(Package package)
        {
            _package = package;

            _viewModel = Ioc.Instance.Resolve<ProjectGroupsViewModel>();

            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandId = new CommandID(CommandSet, BetterStartPageMenuOpenFavouritePlaceholderCommand);
                var menuItem = new ProjectGroupsMenuCommand(menuCommandId,
                    IsValidDynamicItem, OnInvokedDynamicItem, OnBeforeQueryStatusDynamicItem);
                commandService.AddCommand(menuItem);
            }
        }

        private void OnBeforeQueryStatusDynamicItem(object sender, EventArgs e)
        {
            var matchedCommand = (ProjectGroupsMenuCommand)sender;
            matchedCommand.Enabled = true;
            matchedCommand.Visible = true;

            var isRootItem = matchedCommand.MatchedCommandId == 0;
            if (isRootItem || _flattened == null)
            {
                Flatten();
            }

            if (_flattened.Count == 0)
            {
                return;
            }

            var indexForDisplay = isRootItem ? 0 : matchedCommand.MatchedCommandId - BetterStartPageMenuOpenFavouritePlaceholderCommand;

            var x = _flattened[indexForDisplay];
            matchedCommand.Text = x.Item1.Title + " > " + x.Item2.Name;
            matchedCommand.Project = x.Item2;

            matchedCommand.MatchedCommandId = 0;
        }

        private void OnInvokedDynamicItem(object sender, EventArgs e)
        {
            var invokedCommand = (ProjectGroupsMenuCommand)sender;
            _viewModel.OpenProjectCommand.Execute(invokedCommand.Project);
        }

        private bool IsValidDynamicItem(int commandId)
        {
            if (_flattened == null)
            {
                Flatten();
            }
            return (commandId >= BetterStartPageMenuOpenFavouritePlaceholderCommand) &&
                   (commandId - BetterStartPageMenuOpenFavouritePlaceholderCommand < _flattened.Count);
        }

        private void Flatten()
        {
            _flattened = _viewModel.Groups.SelectMany(g => g.Projects.Select(p => Tuple.Create(g, p))).ToList();
        }

        public static void Initialize(Package package)
        {
            Instance = new ProjectGroupsMenu(package);
        }
    }
}
