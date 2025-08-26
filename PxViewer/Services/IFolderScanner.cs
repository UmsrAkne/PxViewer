using System.Collections.Generic;
using System.Threading;
using PxViewer.Models;

namespace PxViewer.Services
{
    public interface IFolderScanner
    {
        IReadOnlyList<ImageEntry> Scan(string folderPath, CancellationToken ct = default);
    }
}