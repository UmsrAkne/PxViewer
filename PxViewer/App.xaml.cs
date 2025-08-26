using System;
using System.IO;
using System.Windows;
using Prism.Ioc;
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

                if (Directory.Exists(imagesDir) && shell.DataContext is MainWindowViewModel vm)
                {
                    var tab = new TabViewModel(new FolderId(imagesDir));
                    vm.Tabs.Add(tab);
                    vm.CurrentTab = tab;
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
        }
    }
}