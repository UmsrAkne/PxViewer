using System;
using System.Collections.Generic;
using System.IO;

namespace PxViewer.Services
{
    public class DirectoryWatcher : IDirectoryWatcher, IDisposable
    {
        private FileSystemWatcher watcher;

        public event Action<string> OnChanged;

        public void Watch(string folderPath)
        {
            watcher?.Dispose();

            watcher = new FileSystemWatcher(folderPath)
            {
                NotifyFilter = NotifyFilters.FileName
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
            };

            watcher.Created += OnChangedInternal;
            watcher.Deleted += OnChangedInternal;
            watcher.Changed += OnChangedInternal;
            watcher.Renamed += OnRenamedInternal;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            watcher.Dispose();
        }

        private void OnChangedInternal(object sender, FileSystemEventArgs e)
        {
            if (!IsSupportedImageExtension(e.FullPath))
            {
                return;
            }

            OnChanged?.Invoke(e.FullPath);
        }

        private void OnRenamedInternal(object sender, RenamedEventArgs e)
        {
            if (!IsSupportedImageExtension(e.FullPath))
            {
                return;
            }

            OnChanged?.Invoke(e.FullPath);
        }

        private bool IsSupportedImageExtension(string fullPath)
        {
            var extension = Path.GetExtension(fullPath);
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            return new List<string>()
            {
                ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".tiff", "psd",
            }.Contains(extension.ToLowerInvariant());
        }
    }
}