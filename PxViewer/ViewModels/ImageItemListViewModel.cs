using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Prism.Mvvm;
using PxViewer.Models;
using PxViewer.Services;

namespace PxViewer.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageItemListViewModel : BindableBase
    {
        private readonly IThumbnailService thumbnailService;

        public ImageItemListViewModel(IThumbnailService thumbnailService)
        {
            this.thumbnailService = thumbnailService;
            FilteredView = CollectionViewSource.GetDefaultView(ImageItems);
            FilteredView.Refresh();
        }

        public ObservableCollection<ImageItemViewModel> ImageItems { get; } = new ();

        public ICollectionView FilteredView { get; set; }

        public async Task CreateImageItem(string fullPath)
        {
            // 重複チェック
            if (ImageItems.Any(x => x.Entry.FullPath == fullPath))
            {
                return;
            }

            var item = CreateImageItemViewModel(fullPath);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ImageItems.Add(item);
            });
        }

        public async Task RemoveImageItem(string fullPath)
        {
            var toRemove = ImageItems.FirstOrDefault(x => x.Entry.FullPath == fullPath);

            if (toRemove == null)
            {
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(() => ImageItems.Remove(toRemove));
        }

        public async Task UpdateImageItem(string fullPath)
        {
            var toUpdate = ImageItems.FirstOrDefault(x => x.Entry.FullPath == fullPath);
            if (toUpdate == null)
            {
                return;
            }

            var fi = new FileInfo(fullPath);
            if (!fi.Exists || fi.LastWriteTimeUtc == toUpdate.Entry.LastWriteUtc)
            {
                return;
            }

            var item = CreateImageItemViewModel(fullPath);
            await item.LoadThumbnailAsync();

            var index = ImageItems.IndexOf(toUpdate);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ImageItems.RemoveAt(index);
                ImageItems.Insert(index, item);
            });
        }

        public async Task RenameImageItem(string oldPath, string newPath)
        {
            var toRename = ImageItems.FirstOrDefault(x => x.Entry.FullPath == oldPath);
            if (toRename == null)
            {
                return;
            }

            var fin = new FileInfo(newPath);
            if (!fin.Exists)
            {
                return;
            }

            var newItem = CreateImageItemViewModel(newPath);
            await newItem.LoadThumbnailAsync();

            // アイテムを置き換える
            var index = ImageItems.IndexOf(toRename);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ImageItems.RemoveAt(index);
                ImageItems.Insert(index, newItem);
            });
        }

        private ImageItemViewModel CreateImageItemViewModel(string fullPath)
        {
            var entry = ImageEntry.FromFile(fullPath);
            var item = new ImageItemViewModel(thumbnailService) { Entry = entry, };
            return item;
        }
    }
}