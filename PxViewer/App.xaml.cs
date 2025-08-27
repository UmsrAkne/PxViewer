using System;
using System.IO;
using System.Windows;
using Prism.Ioc;
using Prism.Mvvm;
using PxViewer.Models;
using PxViewer.ViewModels;
using PxViewer.Views;

namespace PxViewer
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            var shell = Container.Resolve<MainWindow>();

            #if DEBUG
            // デバッグ用のテストデータを MainWindowViewModel に注入する。
            try
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                var imagesDir = Path.Combine(
                    home, "tests", "RiderProjects", "PxViewer", "images");

                if (Directory.Exists(imagesDir) && shell.DataContext is IMainWindowVm vm)
                {
                    var tab = new TabViewModel(new FolderId(imagesDir));
                    vm.Tabs.Add(tab);
                    vm.CurrentTab = tab;
                    vm.CurrentTab.LoadFilesCommand.Execute();
                }
            }
            catch
            {
                /* デバッグ補助なので失敗しても無視 */
            }
            #endif

            return shell;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            #if DEBUG
            // デバッグ時は DesignTimeMainWindowViewModel を登録
            containerRegistry.Register<IMainWindowVm, DesignTimeMainWindowViewModel>();
            #else
            // 本番時は MainWindowViewModel を登録
            containerRegistry.Register<IMainWindowVm, MainWindowViewModel>();
            #endif
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            #if DEBUG
            ViewModelLocationProvider.Register<MainWindow, DesignTimeMainWindowViewModel>();
            #else
            ViewModelLocationProvider.Register<MainWindow, MainWindowViewModel>();
            #endif
        }
    }
}