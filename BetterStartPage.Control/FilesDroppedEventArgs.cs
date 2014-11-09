using System;

namespace BetterStartPage.Control
{
    class FilesDroppedEventArgs : EventArgs
    {
        public object DropTarget { get; private set; }
        public object DropData { get; private set; }

        public FilesDroppedEventArgs(object dropTarget, object dropData)
        {
            DropTarget = dropTarget;
            DropData = dropData;
        }
    }
}
