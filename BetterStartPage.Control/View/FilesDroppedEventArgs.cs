using System;

namespace BetterStartPage.Control.View
{
    class FilesDroppedEventArgs : EventArgs
    {
        public object DropTarget { get; }
        public object DropData { get; }

        public FilesDroppedEventArgs(object dropTarget, object dropData)
        {
            DropTarget = dropTarget;
            DropData = dropData;
        }
    }
}
