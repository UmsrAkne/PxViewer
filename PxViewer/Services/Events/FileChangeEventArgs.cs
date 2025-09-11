namespace PxViewer.Services.Events
{
    public class FileChangeEventArgs
    {
        public string FullPath { get; set; }

        public string? OldPath { get; set; } // Renamed時に使用

        public string ChangeType { get; set; } // 変更の種類: Created, Deleted, Changed, Renamed
    }
}