using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PxViewer.Models;

namespace PxViewer.Services.Dummy
{
    public class DummyCatalogStore : IImageCatalogStore
    {
        public Task UpsertImagesAsync(FolderId folder, IEnumerable<ImageEntry> images, CancellationToken ct = default)
        {
            throw new System.NotImplementedException();
        }

        public Task RemoveMissingAsync(FolderId folder, IEnumerable<string> existingFullPaths, CancellationToken ct = default)
        {
            throw new System.NotImplementedException();
        }

        public IAsyncEnumerable<ImageEntry> QueryByFolderAsync(FolderId folder, CancellationToken ct = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<ImageEntry> FindByPathAsync(string fullPath, CancellationToken ct = default)
        {
            throw new System.NotImplementedException();
        }
    }
}