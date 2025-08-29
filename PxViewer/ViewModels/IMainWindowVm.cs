using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace PxViewer.ViewModels
{
    public interface IMainWindowVm
    {
        string Title { get; }

        ObservableCollection<TabViewModel> Tabs { get; }

        TabViewModel CurrentTab { get; set; }

        public AsyncRelayCommand CreateNewTabCommandAsync { get; }
    }
}