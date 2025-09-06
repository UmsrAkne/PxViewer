using System.Threading.Tasks;
using PxViewer.Models;
using PxViewer.ViewModels;

namespace PxViewer.Services
{
    public class TabService
    {
        private readonly IThumbnailService thumbnailService = new ThumbnailService();

        public async Task<TabViewModel> CreateAndLoadAsync(string folderPath)
        {
            var tab = new TabViewModel(new FolderId(folderPath), thumbnailService);
            await tab.LoadFilesCommand.ExecuteAsync(null);
            return tab;
        }
    }
}