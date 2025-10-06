using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using PxViewer.Models;
using PxViewer.Services;
using PxViewer.Services.Events;
using PxViewer.Utils;
using PxViewer.Views;

namespace PxViewer.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class TabViewModel : BindableBase, IDisposable
    {
        private readonly CancellationTokenSource cts = new();
        private readonly IThumbnailService thumbnailService;
        private readonly IDialogService dialogService;
        private readonly int cacheCapacity = 10;
        private readonly DirectoryWatcher directoryWatcher;
        private string header;
        private string address;
        private ImageItemViewModel selectedItem;
        private PngGenerationMetadata selectedItemMeta;

        public TabViewModel(FolderId folder, IThumbnailService thumbnailService, IDialogService dialogService)
        {
            Folder = folder;
            Address = folder.Value;
            Header = Path.GetFileName(folder.Value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            FolderScanner = new FolderScanner();
            this.thumbnailService = thumbnailService;
            directoryWatcher = new DirectoryWatcher();

            var directoryInfo = new DirectoryInfo(Address);

            if (directoryInfo.Exists)
            {
                directoryWatcher.Watch(directoryInfo.FullName);
                directoryWatcher.OnChanged += DirectoryWatcherOnOnChanged;
            }

            ImageItemListViewModel = new ImageItemListViewModel(thumbnailService);
            this.dialogService = dialogService;
        }

        public FolderId Folder { get; private set; }

        public ImageItemViewModel SelectedItem
        {
            get => selectedItem;
            set
            {
                var old = selectedItem;

                if (!SetProperty(ref selectedItem, value))
                {
                    return;
                }

                RememberOld(old);
                _ = selectedItem?.LoadAsync(previewMax: 800);
                selectedItem = value;
                if (selectedItem == null)
                {
                    SelectedItemMeta = new PngGenerationMetadata() { IsEmpty = true, };
                    return;
                }

                var metaText = PngMetadataReader.ReadPngMetadata(selectedItem.Entry.FullPath);
                SelectedItemMeta = PngMetadataReader.Parse(metaText);
            }
        }

        public PngGenerationMetadata SelectedItemMeta
        {
            get => selectedItemMeta;
            set => SetProperty(ref selectedItemMeta, value);
        }

        public string Header { get => header; set => SetProperty(ref header, value); }

        public string Address { get => address; set => SetProperty(ref address, value); }

        public ImageItemListViewModel ImageItemListViewModel { get; }

        public AsyncRelayCommand LoadFilesCommand => new (InitializeAsync);

        public AsyncRelayCommand<string> ChangeDirectoryAsyncCommand => new (async (param) =>
        {
            var url = param ?? Address;
            if (string.IsNullOrWhiteSpace(url) || !Directory.Exists(url))
            {
                return;
            }

            Folder = new FolderId(url);
            Header = Path.GetFileName(Folder.Value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            Address = Folder.Value;
            await LoadFilesCommand.ExecuteAsync(null);
        });

        public AsyncRelayCommand NavigateToParentDirectoryAsyncCommand => new (async () =>
        {
            var parent = Directory.GetParent(Folder.Value);
            if (parent is not { Exists: true, })
            {
                return;
            }

            Folder = new FolderId(parent.FullName);
            Address = Folder.Value;
            Header = Path.GetFileName(Folder.Value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            await LoadFilesCommand.ExecuteAsync(null);
        });

        public AsyncRelayCommand<ImageItemViewModel> LoadThumbnailsAsyncCommand => new (async item =>
        {
            await item.LoadThumbnailAsync();
        });

        public AsyncRelayCommand<Rating?> RateImageAsyncCommand => new (rate =>
        {
            if (SelectedItem == null)
            {
                return Task.CompletedTask;
            }

            if (rate.HasValue)
            {
                SelectedItem.Rating = rate.Value;
                ImageItemListViewModel.FilteredView.Refresh();
            }

            return Task.CompletedTask;
        });

        public DelegateCommand OpenInputDialogCommand => new (() =>
        {
               dialogService.ShowDialog(nameof(InputDialog), new DialogParameters(), _ => { });
        });

        private IFolderScanner FolderScanner { get; set; }

        private Queue<ImageItemViewModel> PreviewHistory { get; set; } = new ();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            cts.Dispose();
            directoryWatcher.Dispose();
        }

        private async Task InitializeAsync()
        {
            var dispatcher = Application.Current.Dispatcher;
            var batch = new List<ImageEntry>(256);

            dispatcher.Invoke(() => ImageItemListViewModel.ImageItems.Clear());

            // ディレクトリの数はそこまで多くない想定なので、同期的に列挙してリストに詰める。
            var directories = Directory.GetDirectories(Folder.Value);
            var dirVms = directories.Select(d => new ImageItemViewModel(thumbnailService)
                { IsDirectory = true, Entry = ImageEntry.FromDirectory(d), });
            await dispatcher.InvokeAsync(() => ImageItemListViewModel.ImageItems.AddRange(dirVms));

            await Task.Run(
            () =>
            {
                foreach (var e in FolderScanner.Scan(Folder.Value, cts.Token))
                {
                    batch.Add(e);
                    if (batch.Count >= 256)
                    {
                        var toAdd = batch.Select(Selector).ToList();
                        batch.Clear();
                        dispatcher.Invoke(() => ImageItemListViewModel.ImageItems.AddRange(toAdd));
                    }

                    cts.Token.ThrowIfCancellationRequested();
                }
            }, cts.Token);

            if (batch.Count > 0)
            {
                var toAdd = batch.Select(Selector);
                await Application.Current.Dispatcher.InvokeAsync(() => ImageItemListViewModel.ImageItems.AddRange(toAdd));
            }

            return;

            ImageItemViewModel Selector(ImageEntry i) => new (thumbnailService) { Entry = i, };
        }

        private void RememberOld(ImageItemViewModel item)
        {
            if (item == null)
            {
                return;
            }

            // すでに履歴にあるなら入れ直さない
            if (PreviewHistory.Contains(item))
            {
                return;
            }

            PreviewHistory.Enqueue(item);

            // 10件超えたら古い順に解放
            while (PreviewHistory.Count > cacheCapacity)
            {
                var toRelease = PreviewHistory.Dequeue();

                // 表示中のものを誤って解放しないようにする
                if (toRelease == selectedItem)
                {
                    continue;
                }

                toRelease.ReleaseImage();
            }
        }

        // ReSharper disable once AsyncVoidMethod
        private async void DirectoryWatcherOnOnChanged(FileChangeEventArgs e)
        {
            switch (e.ChangeType)
            {
                case FileEventType.Create:
                    await TryAndRunAsync(
                        e.FullPath,
                        () => ImageItemListViewModel.CreateImageItem(e.FullPath));

                    break;
                case FileEventType.Update:
                    await TryAndRunAsync(
                        e.FullPath,
                        () => ImageItemListViewModel.UpdateImageItem(e.FullPath));

                    break;
                case FileEventType.Delete:
                    await ImageItemListViewModel.RemoveImageItem(e.FullPath);
                    break;
                case FileEventType.Rename:
                    await TryAndRunAsync(
                        e.FullPath,
                        () => ImageItemListViewModel.RenameImageItem(e.OldPath, e.FullPath));

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;

            async Task TryAndRunAsync(string fullPath, Func<Task> onSuccess)
            {
                var fileInfo = await TryGetFileInfoWithRetryAsync(fullPath);
                if (fileInfo != null)
                {
                    await onSuccess();
                }
            }
        }

        private async Task<FileInfo> TryGetFileInfoWithRetryAsync(string path, int maxAttempts = 5, int delayMs = 500)
        {
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var fi = new FileInfo(path);
                    await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

                    // ファイルにアクセスできたので return
                    return fi;
                }
                catch (IOException)
                {
                    // 書き込み中の可能性あり、次のループで再試行
                }
                catch (UnauthorizedAccessException)
                {
                    // 他プロセスが握ってる可能性あり、次のループで再試行
                }

                await Task.Delay(delayMs);
            }

            return null; // 再試行してもアクセスできない場合は null を返す
        }
    }
}