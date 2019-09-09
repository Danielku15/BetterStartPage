using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell;

namespace BetterStartPage.Converter
{
    internal class PathToSystemIconConverter : IMultiValueConverter
    {
        private readonly Dictionary<string, ImageSource> _smallImageCache;
        private readonly Dictionary<string, ImageSource> _largeImageCache;
        
        public PathToSystemIconConverter()
        {
            _smallImageCache = new Dictionary<string, ImageSource>();
            _largeImageCache = new Dictionary<string, ImageSource>();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2) return null;

            var fileName = values[0] as string;
            if (fileName == null) return null;

            var smallIcon = values[1] is bool b && b;

            return ConvertFromCache(fileName, smallIcon);
        }

        private object ConvertFromCache(string fileName, bool smallIcon)
        {
            if (Utilities.IsHttp(fileName))
            {
                fileName = "test.url";
            }

            var cache = smallIcon ? _smallImageCache : _largeImageCache;

            var extension = Path.GetExtension(fileName);

            if (!cache.TryGetValue(extension, out var image))
            {
                image = GetFileIcon(fileName, smallIcon);
                cache[extension] = image;
            }

            return image;
        }

        private ImageSource GetFileIcon(string fileName, bool smallIcon)
        {
            // if file does not exist, create a temp file with the same file extension
            var isTemp = false;
            try
            {
                if (!File.Exists(fileName) && !Directory.Exists(fileName))
                {
                    isTemp = true;
                    fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(fileName));
                    File.WriteAllText(fileName, string.Empty);
                }

                var shinfo = new SHFILEINFO();

                var flags = SHGFI_SYSICONINDEX;
                if (fileName.IndexOf(":", StringComparison.Ordinal) == -1)
                {
                    flags = flags | SHGFI_USEFILEATTRIBUTES;
                }
                flags = flags | SHGFI_ICON;
                if (smallIcon)
                {
                    flags |= SHGFI_SMALLICON;
                }

                var hr = SHGetFileInfo(fileName, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo), flags);
                if (hr != IntPtr.Zero)
                {
                    Bitmap bitmap;
                    using (var icon = Icon.FromHandle(shinfo.hIcon))
                    {
                        bitmap = icon.ToBitmap();
                    }
                    DestroyIcon(shinfo.hIcon);

                    var hBitmap = bitmap.GetHbitmap();
                    try
                    {
                        return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                    finally
                    {
                        DeleteObject(hBitmap);
                    }
                }
            }
            catch (Exception e)
            {
                ActivityLog.LogError("BetterStartPage", "Failed to create icon: " + e);
            }
            finally
            {
                if (isTemp)
                {
                    File.Delete(fileName);
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #region PInvoke

        // ReSharper disable InconsistentNaming
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable MemberCanBePrivate.Local

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_SYSICONINDEX = 16384;
        private const uint SHGFI_USEFILEATTRIBUTES = 16;

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
                                                  ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private extern static bool DestroyIcon(IntPtr handle);

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        // ReSharper restore MemberCanBePrivate.Local
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore InconsistentNaming

        #endregion
    }
}