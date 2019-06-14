using System;

namespace BetterStartPage
{
    internal class Utilities
    {
        public static bool IsHttp(string fileName)
        {
            return (fileName.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                    || fileName.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase));
        }
    }
}