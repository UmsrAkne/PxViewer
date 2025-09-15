using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace PxViewer.Models
{
    public class ImageEntry
    {
        public ImageId Id { get; set; }

        public string FullPath { get; set; }

        public DateTime LastWriteUtc { get; set; }

        public long FileSize { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public static ImageEntry FromFile(string path)
        {
            var fi = new FileInfo(path);
            var size = GetImageDimensions(path);

            return new ImageEntry
            {
                Id = new ImageId(path.ToLowerInvariant()),
                FullPath = path,
                LastWriteUtc = fi.LastWriteTimeUtc,
                FileSize = fi.Length,
                Width = size.Width,
                Height = size.Height,
            };
        }

        private static Size GetImageDimensions(string path)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();

            return new Size(bitmap.PixelWidth, bitmap.PixelHeight);
        }
    }
}