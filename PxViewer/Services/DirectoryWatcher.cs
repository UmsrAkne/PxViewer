using System;
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
            OnChanged?.Invoke(e.FullPath);
        }

        private void OnRenamedInternal(object sender, RenamedEventArgs e)
        {
            OnChanged?.Invoke(e.FullPath);
        }
    }
}