using System;

namespace PxViewer.Services
{
    public interface IDirectoryWatcher
    {
        event Action<string> OnChanged; // パス単位で通知

        void Watch(string folderPath);
    }
}