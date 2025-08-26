using System.Collections.ObjectModel;
using System.IO;
using Prism.Mvvm;
using PxViewer.Models;

namespace PxViewer.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TabViewModel : BindableBase
    {
        private string header;

        public TabViewModel(FolderId folder)
        {
            Folder = folder;
            Header = Path.GetFileName(folder.Value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        public FolderId Folder { get; }

        public ObservableCollection<ImageItemViewModel> Thumbnails { get; } = new ();

        public ImageItemViewModel? SelectedItem { get; set; }

        public object? SelectedItemMeta { get; set; }

        public string Header { get => header; set => SetProperty(ref header, value); }
    }
}