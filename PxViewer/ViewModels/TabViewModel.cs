using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
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
        private string address;

        public TabViewModel(FolderId folder)
        {
            Folder = folder;
            Address = folder.Value;
            Header = Path.GetFileName(folder.Value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            FolderScanner = new FolderScanner();
        }

        public FolderId Folder { get; private set; }

        public ObservableCollection<ImageItemViewModel> Thumbnails { get; } = new ();

        public ImageItemViewModel SelectedItem { get; set; }

        public object SelectedItemMeta { get; set; }

        public string Header { get => header; set => SetProperty(ref header, value); }

        public string Address { get => address; set => SetProperty(ref address, value); }

        public AsyncRelayCommand LoadFilesCommand => new (InitializeAsync);

        public AsyncRelayCommand ChangeDirectoryAsyncCommand => new AsyncRelayCommand(async () =>
        {
            if (string.IsNullOrWhiteSpace(Address) || !Directory.Exists(Address))
            {
                return;
            }

            Folder = new FolderId(Address);
            Header = Path.GetFileName(Folder.Value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            await LoadFilesCommand.ExecuteAsync(null);
        });

        private IFolderScanner FolderScanner { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            cts.Dispose();
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
    }
}