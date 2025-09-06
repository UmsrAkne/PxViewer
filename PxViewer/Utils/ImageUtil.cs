using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace PxViewer.Utils
{
    public static class ImageUtil
    {
        public static BitmapImage LoadBitmap(string path, int? maxWidth)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad; // ファイルロック回避
            bmp.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bmp.UriSource = new Uri(path);
            if (maxWidth.HasValue)
            {
                bmp.DecodePixelWidth = maxWidth.Value; // 引数に幅が指定されていれば縮小デコード
            }

            bmp.EndInit();
            bmp.Freeze(); // 他スレッド安全

            System.Diagnostics.Debug.WriteLine($"{maxWidth}　でファイルを読み込み(ImageItemViewModel : 97)");
            return bmp;
        }
    }
}