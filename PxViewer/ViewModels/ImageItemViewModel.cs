using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Prism.Mvvm;
using PxViewer.Models;

namespace PxViewer.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class ImageItemViewModel : BindableBase, IDisposable
    {
        private ImageSource image;
        private CancellationTokenSource loadCts;

        public ImageEntry Entry { get; init; }

        public string ThumbnailPath { get; set; } = string.Empty;

        public ImageSource PreviewSource { get; set; }

        public string FileName => Path.GetFileName(Entry.FullPath);

        public ImageSource Image { get => image; private set => SetProperty(ref image, value); }

        public void CancelLoad()
        {
            loadCts?.Cancel();
            loadCts?.Dispose();
            loadCts = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task LoadAsync(int previewMax = 800, bool alsoLoadFull = true)
        {
            CancelLoad();
            loadCts = new CancellationTokenSource();
            var ct = loadCts.Token;

            // 1) 軽量プレビュー同期処理ですぐ表示
            var preview = await Task.Run(() => LoadBitmap(Entry.FullPath, previewMax), ct);
            if (ct.IsCancellationRequested)
            {
                return;
            }

            Image = preview;

            if (!alsoLoadFull)
            {
                return;
            }

            // 2) 裏でフル解像度まだ同じアイテムなら差し替え
            var full = await Task.Run(() => LoadBitmap(Entry.FullPath, null), ct);
            if (ct.IsCancellationRequested)
            {
                return;
            }

            Image = full;
        }

        protected virtual void Dispose(bool disposing)
        {
            loadCts.Dispose();
        }

        private static BitmapImage LoadBitmap(string path, int? maxWidth)
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