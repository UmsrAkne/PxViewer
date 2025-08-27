using System.Collections.ObjectModel;
using Prism.Mvvm;
using PxViewer.Utils;

namespace PxViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase, IMainWindowVm
    {
        private readonly AppVersionInfo appVersionInfo = new ();

        public string Title => appVersionInfo.GetAppNameWithVersion();

        public ObservableCollection<TabViewModel> Tabs { get; } = new ();

        public TabViewModel CurrentTab { get; set; }
    }
}