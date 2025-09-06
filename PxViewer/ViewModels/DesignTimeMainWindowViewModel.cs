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
        private readonly IThumbnailService thumbnailService = new ThumbnailService();
        private TabViewModel currentTab;

        public DesignTimeMainWindowViewModel()
        {
            Tabs.Add(CreateTab("testPath1"));
            Tabs.Add(CreateTab("testPath2"));
            Tabs.Add(CreateTab("testPath3"));

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

        private TabViewModel CreateTab(string path)
        {
            return new TabViewModel(new FolderId(path), thumbnailService);
        }

        [Conditional("DEBUG")]
        private void InjectDummies()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var imagesDir = Path.Combine(
                home, "tests", "RiderProjects", "PxViewer", "images");

            var tab = CreateTab(imagesDir);
            Tabs.Add(tab);
            CurrentTab = tab;
            CurrentTab.LoadFilesCommand.Execute(null);

            Tabs.Add(CreateTab(imagesDir));
        }
    }
}