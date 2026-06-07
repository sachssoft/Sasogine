using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Loaders
{
    public interface IFileSource
    {
        string? FilePath { get; set; }

        Stream GetStream();

        Task<Stream> GetStreamAsync();
    }
}
