using System;
using PxViewer.Services.Events;

namespace PxViewer.Services
{
    public interface IDirectoryWatcher
    {
        event Action<FileChangeEventArgs> OnChanged; // パス単位で通知

        void Watch(string folderPath);
    }
}