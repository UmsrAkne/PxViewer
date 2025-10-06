using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using PxViewer.Services;
using PxViewer.Utils;

namespace PxViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase, IMainWindowVm
    {
        private readonly AppVersionInfo appVersionInfo = new ();
        private readonly TabService tabService = new ();

        public MainWindowViewModel(IDialogService dialogService)
        {
            tabService.DialogService = dialogService;
        }

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