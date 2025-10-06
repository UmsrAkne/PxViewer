using System.Threading.Tasks;
using Prism.Services.Dialogs;
using PxViewer.Models;
using PxViewer.ViewModels;

namespace PxViewer.Services
{
    public class TabService
    {
        private readonly IThumbnailService thumbnailService = new ThumbnailService();

        public IDialogService DialogService { private get; set; }

        public async Task<TabViewModel> CreateAndLoadAsync(string folderPath)
        {
            var tab = new TabViewModel(new FolderId(folderPath), thumbnailService, DialogService);
            await tab.LoadFilesCommand.ExecuteAsync(null);
            return tab;
        }
    }
}