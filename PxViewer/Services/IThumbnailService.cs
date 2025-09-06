using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PxViewer.Models;

namespace PxViewer.Services
{
    // サムネイル生成と取得の窓口を担うクラス
    // - 指定された画像から、指定サイズのサムネイルを生成する
    // - 既に生成済みならキャッシュを返す
    // - 必要ならディスクに保存して再利用可能にする
    public interface IThumbnailService
    {
        Task<IReadOnlyList<ImageEntry>> ScanAsync(string folderPath, CancellationToken ct = default);

        Task<string> GetOrCreateThumbnailPath(string imagePath);
    }
}