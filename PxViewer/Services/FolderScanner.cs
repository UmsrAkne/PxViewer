using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using PxViewer.Models;

namespace PxViewer.Services
{
    public class FolderScanner : IFolderScanner
    {
        // ReSharper disable once ArrangeModifiersOrder
        private static readonly HashSet<string> SupportedExtensions = new (StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".tiff",
        };

        public IReadOnlyList<ImageEntry> Scan(
            string folderPath,
            CancellationToken ct = default)
        {
            if (!Directory.Exists(folderPath))
            {
                return Array.Empty<ImageEntry>();
            }

            var results = new List<ImageEntry>();

            foreach (var file in Directory.EnumerateFiles(folderPath))
            {
                ct.ThrowIfCancellationRequested();

                var ext = Path.GetExtension(file);
                if (!SupportedExtensions.Contains(ext))
                {
                    continue;
                }

                var fi = new FileInfo(file);

                results.Add(new ImageEntry
                {
                    Id = new ImageId(file.ToLowerInvariant()),
                    FullPath = file,
                    LastWriteUtc = fi.LastWriteTimeUtc,
                    FileSize = fi.Length,
                });
            }

            return results;
        }
    }
}