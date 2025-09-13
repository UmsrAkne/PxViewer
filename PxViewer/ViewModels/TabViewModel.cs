using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
using PxViewer.Models;
using PxViewer.Services;
using PxViewer.Services.Events;

namespace PxViewer.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class TabViewModel : BindableBase, IDisposable
    {
        private readonly CancellationTokenSource cts = new();
        private readonly IThumbnailService thumbnailService;
        private readonly int cacheCapacity = 10;
        private readonly DirectoryWatcher directoryWatcher;
        private string header;
        private string address;
        private ImageItemViewModel selectedItem;

        public TabViewModel(FolderId folder, IThumbnailService thumbnailService)
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
            }
        }

        public object SelectedItemMeta { get; set; }

        public string Header { get => header; set => SetProperty(ref header, value); }

        public string Address { get => address; set => SetProperty(ref address, value); }

        public ImageItemListViewModel ImageItemListViewModel { get; }

        public AsyncRelayCommand LoadFilesCommand => new (InitializeAsync);

        public AsyncRelayCommand ChangeDirectoryAsyncCommand => new AsyncRelayCommand(async () =>
        {
            if (string.IsNullOrWhiteSpace(Address) || !Directory.Exists(Address))
            {
                return;
            }

            Folder = new FolderId(Address);
            Header = Path.GetFileName(Folder.Value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            await LoadFilesCommand.ExecuteAsync(null);
        });

        public AsyncRelayCommand<ImageItemViewModel> LoadThumbnailsAsyncCommand => new (async item =>
        {
            await item.LoadThumbnailAsync();
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

            await Task.Run(
            () =>
            {
                foreach (var e in FolderScanner.Scan(Folder.Value, cts.Token))
                {
                    batch.Add(e);
                    if (batch.Count >= 256)
                    {
                        var toAdd = batch.Select(Selector);
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
            if (e.ChangeType != FileEventType.Create)
            {
                return;
            }

            await ImageItemListViewModel.CreateImageItem(e.FullPath);
        }
    }
}