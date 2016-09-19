# BetterStartPage

This is a VS2013 like start page which replaces the news section by a custom project dashboard.
In the project dashboard you can organize your solutions and projects into different groups for fast access. 

To edit the dashboard right-click on the empty area and enable the edit mode in the context menu. 
Add new groups by clicking on the "Add Group" link on the bottom, add new projects to the group by dragging Solution or Project files from the explorer to the groups. 
You can reorder or delete groups and projects by using the action buttons beside the items. 

All added items are also accessible via the File > Open Favourite menu item. 

![Screenshot](https://github.com/Danielku15/BetterStartPage/blob/master/BetterStartPage/startpagepreview.png?raw=1 "Screenshot")

# Getting Started

## Installation

- Install the Extension from the [Visual Studio Extension Gallery](https://visualstudiogallery.msdn.microsoft.com/8da4b080-2ad6-47fd-a1ff-4e7cc185523b)
- Go to Tools->Options->Environment->Startup and select the BetterStartPage as start page. 

## Basic Usage

**Due to some restrictions within Windows, it's not possible to drag&drop items from 
  a non elevated application (like the normal Windows Explorer) into an elevated one. 
  Please ensure you start Visual Studio with normal user credentials and not using 
  'Run as Administrator', otherwise the dragging projects and solutions into 
  BetterStartPage will not work**

- right click on the empty area and enable the edit mode 
- add new groups using the "Add Group" link on the bottom
- edit the group titles using the inline textbox
- add new solution and project files to the groups using Drag&Drop from Windows Explorer
- adjust the number of group columns you want to have
- use the action buttons to reorder or delete items 
- right click on the empty area and disable the edit mode to save your layout 
