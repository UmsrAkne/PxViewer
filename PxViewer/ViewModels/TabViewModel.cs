using System.Collections.ObjectModel;
using Prism.Mvvm;
using PxViewer.Models;

namespace PxViewer.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TabViewModel : BindableBase
    {
        public FolderId Folder { get; }

        public ObservableCollection<ImageItemViewModel> Thumbnails { get; } = new ();

        public ImageItemViewModel? SelectedItem { get; set; }

        public object? SelectedItemMeta { get; set; }
    }
}