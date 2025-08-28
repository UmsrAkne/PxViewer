using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using PxViewer.Models;
using PxViewer.Services;

namespace PxViewer.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class TabViewModel : BindableBase, IDisposable
    {
        private readonly CancellationTokenSource cts = new();
        private string header;

        public TabViewModel(FolderId folder)
        {
            Folder = folder;
            Header = Path.GetFileName(folder.Value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            FolderScanner = new FolderScanner();
        }

        public FolderId Folder { get; }

        public ObservableCollection<ImageItemViewModel> Thumbnails { get; } = new ();

        public ImageItemViewModel? SelectedItem { get; set; }

        public object? SelectedItemMeta { get; set; }

        public string Header { get => header; set => SetProperty(ref header, value); }

        public DelegateCommand LoadFilesCommand => new DelegateCommand(() =>
        {
            _ = InitializeAsync();
        });

        private IFolderScanner FolderScanner { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private async Task InitializeAsync()
        {
            var dispatcher = Application.Current.Dispatcher;
            var batch = new List<ImageEntry>(256);

            dispatcher.Invoke(() => Thumbnails.Clear());

            await Task.Run(
            () =>
            {
                foreach (var e in FolderScanner.Scan(Folder.Value, cts.Token))
                {
                    batch.Add(e);
                    if (batch.Count >= 256)
                    {
                        var toAdd = batch.Select(i => new ImageItemViewModel { Entry = i, });
                        batch.Clear();
                        dispatcher.Invoke(() => Thumbnails.AddRange(toAdd));
                    }

                    cts.Token.ThrowIfCancellationRequested();
                }
            }, cts.Token);

            if (batch.Count > 0)
            {
                var toAdd = batch.Select(i => new ImageItemViewModel { Entry = i, });
                await Application.Current.Dispatcher.InvokeAsync(() => Thumbnails.AddRange(toAdd));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            cts.Dispose();
        }
    }
}