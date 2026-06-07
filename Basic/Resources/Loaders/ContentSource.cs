using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Loaders
{
    public class ContentSource : ResourceSourceBase
    {
        protected override Stream OpenStream()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<Stream> OpenStreamAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
