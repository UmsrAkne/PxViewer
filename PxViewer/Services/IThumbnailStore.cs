using System.Threading.Tasks;

namespace PxViewer.Services
{
    // サムネイルファイルの保存先管理（カタログではなくストレージ係）
    // - 「このサムネイルキー（画像パス＋更新時刻＋サイズのハッシュ）に対応するファイルはどこ？」を答える
    // - 新しいサムネイルを作るときに保存先のファイルパスを予約する
    // - 実際のファイルの存在確認を行う
    public interface IThumbnailStore
    {
        bool TryGetPath(string thumbKey, out string path);

        Task<string> ReservePathAsync(string thumbKey);
    }
}