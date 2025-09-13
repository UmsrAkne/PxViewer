using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        }

        public ObservableCollection<ImageItemViewModel> ImageItems { get; } = new ();

        public async Task CreateImageItem(string fullPath)
        {
            // 重複チェック
            if (ImageItems.Any(x => x.Entry.FullPath == fullPath))
            {
                return;
            }

            var entry = new ImageEntry() { FullPath = fullPath, };
            var item = new ImageItemViewModel(thumbnailService) { Entry = entry, };

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
    }
}