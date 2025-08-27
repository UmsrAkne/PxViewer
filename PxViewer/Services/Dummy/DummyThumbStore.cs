using System.Threading.Tasks;

namespace PxViewer.Services.Dummy
{
    public class DummyThumbStore : IThumbnailStore
    {
        public bool TryGetPath(string thumbKey, out string path)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> ReservePathAsync(string thumbKey)
        {
            throw new System.NotImplementedException();
        }
    }
}