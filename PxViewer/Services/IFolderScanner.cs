using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PxViewer.Models;

namespace PxViewer.Services
{
    public interface IFolderScanner
    {
        Task<IReadOnlyList<ImageEntry>> ScanAsync(string folderPath, CancellationToken ct = default);
    }
}