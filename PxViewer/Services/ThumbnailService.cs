using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PxViewer.Models;

namespace PxViewer.Services
{
    public class ThumbnailService : IThumbnailService
    {
        public Task<IReadOnlyList<ImageEntry>> ScanAsync(string folderPath, CancellationToken ct = default)
        {
            throw new System.NotImplementedException();
        }

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
                System.Diagnostics.Debug.WriteLine($"サムネイルパスの取得に失敗しました。(ImageItemViewModel : 59)");
                return string.Empty;
            }

            if (File.Exists(thumbPath))
            {
                return thumbPath;
            }

            var thumbnail = await Task.Run(() => LoadBitmap(imagePath, 256));
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(thumbnail));
            await using var stream = new FileStream(thumbPath, FileMode.Create);
            encoder.Save(stream);

            return thumbPath;
        }

        public BitmapImage LoadBitmap(string path, int? maxWidth)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad; // ファイルロック回避
            bmp.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bmp.UriSource = new Uri(path);
            if (maxWidth.HasValue)
            {
                bmp.DecodePixelWidth = maxWidth.Value; // 引数に幅が指定されていれば縮小デコード
            }

            bmp.EndInit();
            bmp.Freeze(); // 他スレッド安全

            System.Diagnostics.Debug.WriteLine($"{maxWidth}　でファイルを読み込み(ImageItemViewModel : 97)");
            return bmp;
        }
    }
}