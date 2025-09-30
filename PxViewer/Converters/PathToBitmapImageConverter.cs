using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PxViewer.Utils;

namespace PxViewer.Converters
{
    public class PathToBitmapImageConverter : IValueConverter
    {
        private int MaxDecodePixels { get; set; } = 600;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (File.Exists(path))
            {
                var max = MaxDecodePixels;
                if (parameter is string s && int.TryParse(s, out var p))
                {
                    max = p;
                }

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bmp.UriSource = new Uri(path);
                bmp.DecodePixelWidth = max;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }

            return Directory.Exists(path) ? GetFolderIconImageSource() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private ImageSource GetFolderIconImageSource()
        {
            const uint shGetFileInfoFlags = 0x100;

            _ = ShellInterop.NativeMethods.SHGetFileInfo(
                @"C:\Windows", // 適当な既存ディレクトリ（中身は関係ない）
                0,
                out var shellFileInfo,
                (uint)Marshal.SizeOf(typeof(ShellInterop.NativeMethods.ShFileInfo)),
                shGetFileInfoFlags);

            if (shellFileInfo.hIcon == IntPtr.Zero)
            {
                return null;
            }

            var img = Imaging.CreateBitmapSourceFromHIcon(
                shellFileInfo.hIcon,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(96, 96));

            img.Freeze();
            ShellInterop.NativeMethods.DestroyIcon(shellFileInfo.hIcon);

            return img;
        }
    }
}