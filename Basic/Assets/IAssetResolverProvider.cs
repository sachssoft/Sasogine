using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    public interface IAssetResolverProvider
    {
        IAsset? Resolve(Stream stream);
    }
}
