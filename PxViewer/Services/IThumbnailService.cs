using System.Threading.Tasks;

namespace PxViewer.Services
{
    // サムネイル生成と取得の窓口を担うクラス
    // - 指定された画像から、指定サイズのサムネイルを生成する
    // - 既に生成済みならキャッシュを返す
    // - 必要ならディスクに保存して再利用可能にする
    public interface IThumbnailService
    {
        Task<string> GetOrCreateThumbnailPath(string imagePath);
    }
}