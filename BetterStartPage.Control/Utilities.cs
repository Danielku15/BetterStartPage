using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace BetterStartPage.Control
{
    internal class Utilities
    {
        public static DTE2 GetDTE()
        {
            return Package.GetGlobalService(typeof(DTE)) as DTE2;
        }
        
        public static bool IsHttp(string fileName)
        {
            return (fileName.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                    || fileName.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase));
        }
    }
}