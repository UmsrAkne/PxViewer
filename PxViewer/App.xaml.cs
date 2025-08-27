using System.Windows;
using Prism.Ioc;
using Prism.Mvvm;
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
            return Container.Resolve<MainWindow>();
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