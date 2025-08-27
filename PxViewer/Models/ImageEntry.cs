using System;

namespace PxViewer.Models
{
    public class ImageEntry
    {
        public ImageId Id { get; set; }

        public string FullPath { get; set; }

        public DateTime LastWriteUtc { get; set; }

        public long FileSize { get; set; }
    }
}