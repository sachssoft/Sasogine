using Sachssoft.Sasogine.Assets;

namespace Sachssoft.Sasogine.Containers
{
    public interface IPackageAssetFactory
    {
        IAsset? Build(PackageBase package, PackageAssetEntry entry);
    }
}
