using System.Windows.Media;
using Prism.Mvvm;
using PxViewer.Models;

namespace PxViewer.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageItemViewModel : BindableBase
    {
        public ImageEntry Entry { get; set; }

        public string ThumbnailPath { get; set; } = "";

        public ImageSource PreviewSource { get; set; }
    }
}