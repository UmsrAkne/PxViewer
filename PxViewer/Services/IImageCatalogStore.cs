using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PxViewer.Models;

namespace PxViewer.Services
{
    // 画像メタ情報の永続化ストア
    // - DB（最初はJSONやLiteDB、後でEF Core）に「このフォルダにこれらの画像があるよ」という情報を記録
    // - ファイル削除や追加を検知して差分を更新する
    // - 検索やソートで画像リストを返す
    public interface IImageCatalogStore
    {
        Task UpsertImagesAsync(FolderId folder, IEnumerable<ImageEntry> images, CancellationToken ct = default);

        Task RemoveMissingAsync(FolderId folder, IEnumerable<string> existingFullPaths, CancellationToken ct = default);

        IAsyncEnumerable<ImageEntry> QueryByFolderAsync(FolderId folder, CancellationToken ct = default);

        Task<ImageEntry> FindByPathAsync(string fullPath, CancellationToken ct = default);
    }
}