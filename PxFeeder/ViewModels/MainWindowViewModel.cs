using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace PxFeeder.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly string appName = "PxFeeder";
        private string title = "PxFeeder";
        private string sourceFilePath;
        private string destDirectoryPath;
        private int intervalSec = 4;
        private bool isRunning;

        public string Title { get => title; private set => SetProperty(ref title, value); }

        public MainWindowViewModel()
        {
            var testDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "tests", "RiderProjects", "PxViewer");
            SourceFilePath = Path.Combine(testDirectory, "sourceImages", "1.png");
            DestDirectoryPath = Path.Combine(testDirectory,"images");
        }

        public string SourceFilePath { get => sourceFilePath; set => SetProperty(ref sourceFilePath, value); }

        public string DestDirectoryPath
        {
            get => destDirectoryPath;
            set => SetProperty(ref destDirectoryPath, value);
        }

        public int IntervalSec { get => intervalSec; set => SetProperty(ref intervalSec, value); }

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                SetProperty(ref isRunning, value);
                Title = value ? $"{appName} (Running)" : $"{appName}";
            }
        }

        private CancellationTokenSource cts;

        public IAsyncRelayCommand StartCommand => new AsyncRelayCommand(StartCopyLoopAsync);
        public ICommand StopCommand => new RelayCommand(Stop);

        private async Task StartCopyLoopAsync()
        {
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;
            cts = new CancellationTokenSource();

            try
            {
                await FileCopyLoop(SourceFilePath, DestDirectoryPath, TimeSpan.FromSeconds(IntervalSec), cts.Token);
            }
            catch (OperationCanceledException)
            {
                // expected
            }
            finally
            {
                IsRunning = false;
                cts.Dispose();
                cts = null;
            }
        }

        public DelegateCommand OpenExplorerCommand => new DelegateCommand(() =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = DestDirectoryPath,
                UseShellExecute = true,
            });
        });

        private void Stop()
        {
            if (IsRunning)
            {
                cts?.Cancel();
            }
        }

        private async Task FileCopyLoop(string sourceFile, string destDir, TimeSpan interval, CancellationToken ct)
        {
            if (!File.Exists(sourceFile))
            {
                Console.WriteLine($"Source file does not exist: {sourceFile}");
                return;
            }

            if (!Directory.Exists(destDir))
            {
                Console.WriteLine($"Destination directory does not exist: {destDir}");
                return;
            }


            while (!ct.IsCancellationRequested)
            {
                var tempFilePath = Path.Combine(destDir, "temp");

                try
                {
                    await using (var _ = File.Create(tempFilePath))
                    {
                        await Task.Delay(1000, ct);
                    }

                    File.Delete(tempFilePath);

                    var timestamp = DateTime.Now.ToString("HHmmss");
                    var fileName = Path.GetFileNameWithoutExtension(sourceFile);
                    var ext = Path.GetExtension(sourceFile);
                    var destPath = Path.Combine(destDir, $"{fileName}_{timestamp}{ext}");

                    File.Copy(sourceFile, destPath, true);
                    Console.WriteLine($"Copied to: {destPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message}");
                }
                finally
                {
                    if (File.Exists(tempFilePath))
                    {
                        try
                        {
                            File.Delete(tempFilePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARN] Failed to delete temp file: {ex.Message}");
                        }
                    }
                }

                await Task.Delay(interval);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            cts.Dispose();
        }
    }
}