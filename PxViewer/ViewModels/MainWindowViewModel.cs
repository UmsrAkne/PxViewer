using Prism.Mvvm;
using PxViewer.Utils;

namespace PxViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly AppVersionInfo appVersionInfo = new ();

        public string Title => appVersionInfo.GetAppNameWithVersion();
    }
}