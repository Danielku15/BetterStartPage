using Microsoft.VisualStudio.Shell;

namespace BetterStartPage.Control
{
    internal class VsResources
    {
        public static object LinkStyleKey { get; set; }
        public static object LinkHoverStyleKey { get; set; }
        public static object DirectoryLinkStyleKey { get; set; }
        public static object StartPageTabBackgroundKey { get; set; }
        public static object GroupHeaderStyleKey { get; set; }
        public static object GroupHeaderForegroundKey { get; set; }

        static VsResources()
        {
            LinkStyleKey = VsBrushes.StartPageTextControlLinkSelectedKey;
            LinkHoverStyleKey = VsBrushes.StartPageTextControlLinkSelectedHoverKey;
            DirectoryLinkStyleKey = VsBrushes.StartPageTextHeadingKey;
            StartPageTabBackgroundKey = VsBrushes.StartPageTabBackgroundKey;
            GroupHeaderStyleKey = "StartPage.AnnouncementsHeadingTextStyle";
            GroupHeaderForegroundKey = "";
        }
    }
}
