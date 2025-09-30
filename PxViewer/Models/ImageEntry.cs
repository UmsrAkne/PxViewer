using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Prism.Mvvm;

namespace PxViewer.Models
{
    public class ImageEntry : BindableBase
    {
        private Rating rating;

        public ImageId Id { get; set; }

        public string FullPath { get; set; }

        public DateTime LastWriteUtc { get; set; }

        public long FileSize { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public Rating Rating
        {
            get => rating;
            set => SetProperty(ref rating, value);
        }

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

        public static ImageEntry FromDirectory(string path)
        {
            var fi = new DirectoryInfo(path);

            return new ImageEntry
            {
                Id = new ImageId(path.ToLowerInvariant()),
                FullPath = path,
                LastWriteUtc = fi.LastWriteTimeUtc,
            };
        }

        private static Size GetImageDimensions(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
            var frame = decoder.Frames[0];
            return new Size(frame.PixelWidth, frame.PixelHeight);
        }
    }
}