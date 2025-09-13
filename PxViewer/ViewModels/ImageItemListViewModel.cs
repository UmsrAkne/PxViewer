using System.Collections.ObjectModel;
using Prism.Mvvm;
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
    }
}