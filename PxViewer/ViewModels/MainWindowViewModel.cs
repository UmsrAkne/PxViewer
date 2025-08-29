using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
using PxViewer.Services;
using PxViewer.Utils;

namespace PxViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase, IMainWindowVm
    {
        private readonly AppVersionInfo appVersionInfo = new ();
        private readonly TabService tabService = new ();

        public string Title => appVersionInfo.GetAppNameWithVersion();

        public ObservableCollection<TabViewModel> Tabs { get; } = new ();

        public TabViewModel CurrentTab { get; set; }

        public AsyncRelayCommand CreateNewTabCommandAsync => new (async () =>
        {
            var toAdd = await tabService.CreateAndLoadAsync(CurrentTab.Folder.Value);
            Tabs.Add(toAdd);
            CurrentTab = toAdd;
        });
    }
}