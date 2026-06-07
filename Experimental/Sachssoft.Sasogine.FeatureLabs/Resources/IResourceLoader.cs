using System.IO;

namespace Sachssoft.Sasogine.Resources;

public interface IResourceLoader
{
    Stream GetStream();
}
