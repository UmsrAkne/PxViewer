using Prism.Mvvm;

namespace PxFeeder.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string title = "PxFeeder";

        public string Title { get => title; set => SetProperty(ref title, value); }

        public MainWindowViewModel()
        {
        }
    }
}