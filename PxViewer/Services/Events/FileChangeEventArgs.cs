namespace PxViewer.Services.Events
{
    public class FileChangeEventArgs
    {
        public string FullPath { get; init; } = string.Empty;

        public string OldPath { get; init; } = string.Empty; // Renamed時に使用

        public FileEventType ChangeType { get; init; }
    }
}