using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using PxViewer.Models;

namespace PxViewer.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DesignTimeMainWindowViewModel : BindableBase, IMainWindowVm
    {
        private TabViewModel currentTab;

        public DesignTimeMainWindowViewModel()
        {
            Tabs.Add(new TabViewModel(new FolderId("testPath1")));
            Tabs.Add(new TabViewModel(new FolderId("testPath2")));
            Tabs.Add(new TabViewModel(new FolderId("testPath3")));

            CurrentTab = Tabs.First();
        }

        public string Title { get; } = "PxViewer (Debug Mode)";

        public ObservableCollection<TabViewModel> Tabs { get; } = new ();

        public TabViewModel CurrentTab { get => currentTab; set => SetProperty(ref currentTab, value); }
    }
}