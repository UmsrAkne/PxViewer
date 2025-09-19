using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Mvvm;
using PxViewer.Models;
using PxViewer.Services;
using PxViewer.Utils;

namespace PxViewer.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class ImageItemViewModel : BindableBase, IDisposable
    {
        private readonly IThumbnailService thumbnailService;
        private ImageSource image;
        private CancellationTokenSource loadCts;
        private string thumbnailPath = string.Empty;
        private Rating rating;

        public ImageItemViewModel(IThumbnailService thumbnailService)
        {
            this.thumbnailService = thumbnailService;
        }

        public ImageEntry Entry { get; init; }

        public string ThumbnailPath
        {
            get => thumbnailPath;
            set => SetProperty(ref thumbnailPath, value);
        }

        public ImageSource PreviewSource { get; set; }

        public string FileName => Path.GetFileName(Entry.FullPath);

        public ImageSource Image { get => image; private set => SetProperty(ref image, value); }

        public Rating Rating
        {
            get => Entry.Rating;
            set
            {
                Entry.Rating = value;
                SetProperty(ref rating, value);
            }
        }

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

        public async Task LoadThumbnailAsync(int maxWidth = 256)
        {
            var path = await thumbnailService.GetOrCreateThumbnailPath(Entry.FullPath);
            ThumbnailPath = path;
        }

        public async Task LoadAsync(int previewMax = 800, bool alsoLoadFull = true)
        {
            CancelLoad();
            loadCts = new CancellationTokenSource();
            var ct = loadCts.Token;

            try
            {
                // 1) 軽量プレビューを非同期で読み込み
                var preview = await Task.Run(() => ImageUtil.LoadBitmap(Entry.FullPath, previewMax, ct), ct);
                Image = preview;

                if (!alsoLoadFull)
                {
                    return;
                }

                // 2) フル解像度をさらに読み込み（まだ同じアイテムだったら）
                var full = await Task.Run(() => ImageUtil.LoadBitmap(Entry.FullPath, null, ct), ct);
                Image = full;
            }
            catch (OperationCanceledException)
            {
                // キャンセルされても異常ではないため、特にメッセージは出さない。
            }
        }

        public void ReleaseImage()
        {
            Image = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            loadCts.Dispose();
        }
    }
}