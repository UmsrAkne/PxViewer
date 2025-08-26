using System.Collections.ObjectModel;

namespace PxViewer.ViewModels
{
    public interface IMainWindowVm
    {
        string Title { get; }

        ObservableCollection<TabViewModel> Tabs { get; }

        TabViewModel CurrentTab { get; set; }
    }
}