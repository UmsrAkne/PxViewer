using System;
using System.Collections.Generic;
using System.IO;
using PxViewer.Services.Events;

namespace PxViewer.Services
{
    public class DirectoryWatcher : IDirectoryWatcher, IDisposable
    {
        private FileSystemWatcher watcher;

        public event Action<FileChangeEventArgs> OnChanged;

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

            var changeType = e.ChangeType.ToString() switch
            {
                "Created" => FileEventType.Create,
                "Deleted" => FileEventType.Deletee,
                _ => FileEventType.Update,
            };

            OnChanged?.Invoke(new FileChangeEventArgs
            {
                FullPath = e.FullPath,
                ChangeType = changeType,
            });
        }

        private void OnRenamedInternal(object sender, RenamedEventArgs e)
        {
            if (!IsSupportedImageExtension(e.FullPath))
            {
                return;
            }

            OnChanged?.Invoke(new FileChangeEventArgs
            {
                FullPath = e.FullPath,
                ChangeType = FileEventType.Rename,
                OldPath = e.OldFullPath,
            });
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