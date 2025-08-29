using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
using PxViewer.Models;
using PxViewer.Services;

namespace PxViewer.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DesignTimeMainWindowViewModel : BindableBase, IMainWindowVm
    {
        private readonly TabService tabService = new ();
        private TabViewModel currentTab;

        public DesignTimeMainWindowViewModel()
        {
            Tabs.Add(new TabViewModel(new FolderId("testPath1")));
            Tabs.Add(new TabViewModel(new FolderId("testPath2")));
            Tabs.Add(new TabViewModel(new FolderId("testPath3")));

            CurrentTab = Tabs.First();

            InjectDummies();
        }

        public string Title { get; } = "PxViewer (Debug Mode)";

        public ObservableCollection<TabViewModel> Tabs { get; } = new ();

        public TabViewModel CurrentTab { get => currentTab; set => SetProperty(ref currentTab, value); }

        public AsyncRelayCommand CreateNewTabCommandAsync => new (async () =>
        {
            var toAdd = await tabService.CreateAndLoadAsync(CurrentTab.Folder.Value);
            Tabs.Add(toAdd);
            CurrentTab = toAdd;
        });

        [Conditional("DEBUG")]
        private void InjectDummies()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var imagesDir = Path.Combine(
                home, "tests", "RiderProjects", "PxViewer", "images");

            var tab = new TabViewModel(new FolderId(imagesDir));
            Tabs.Add(tab);
            CurrentTab = tab;
            CurrentTab.LoadFilesCommand.Execute(null);

            Tabs.Add(new TabViewModel(new FolderId($"{imagesDir}_2")));
        }
    }
}