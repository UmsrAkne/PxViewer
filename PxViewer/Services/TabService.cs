using System.Threading.Tasks;
using PxViewer.Models;
using PxViewer.ViewModels;

namespace PxViewer.Services
{
    public class TabService
    {
        public async Task<TabViewModel> CreateAndLoadAsync(string folderPath)
        {
            var tab = new TabViewModel(new FolderId(folderPath));
            await tab.LoadFilesCommand.ExecuteAsync(null);
            return tab;
        }
    }
}