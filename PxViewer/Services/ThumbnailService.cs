using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PxViewer.Utils;

namespace PxViewer.Services
{
    public class ThumbnailService : IThumbnailService
    {
        /// <summary>
        /// 入力された画像のサムネイルのパスを取得します。
        /// 既存のサムネイルが存在する場合はそのパスを、存在しない場合は新規作成してそのパスを返します。
        /// </summary>
        /// <param name="imagePath">サムネイルのパスを取得したい画像のパス</param>
        /// <returns>サムネイルのパス</returns>
        public async Task<string> GetOrCreateThumbnailPath(string imagePath)
        {
            var baseDir = Directory.CreateDirectory("thumbnails");

            var thumbPath = ThumbnailHelper.GetThumbnailCachePath(
                ThumbnailHelper.GenerateThumbnailHash(imagePath),
                baseDir.FullName);

            if (string.IsNullOrWhiteSpace(thumbPath))
            {
                System.Diagnostics.Debug.WriteLine($"サムネイルパスの取得に失敗しました。(ThumbnailService)");
                return string.Empty;
            }

            if (File.Exists(thumbPath))
            {
                return thumbPath;
            }

            var thumbnail = await Task.Run(() => ImageUtil.LoadBitmap(imagePath, 256));
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(thumbnail));
            await using var stream = new FileStream(thumbPath, FileMode.Create);
            encoder.Save(stream);

            return thumbPath;
        }
    }
}