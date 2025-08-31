using Sachssoft.Sasogine.Assets;

namespace Sachssoft.Sasogine.Containers
{
    public interface IPackageAssetFactory
    {
        IAssetProvider? Build(PackageBase package, PackageAssetEntry entry);
    }
}
