using System;

namespace Sachssoft.Sasogine.Containers
{
    public sealed class PackageLevelFactory : IPackageLevelFactory
    {
        private readonly Func<PackageBase, string, PackageLevelBase> _factory;

        private PackageLevelFactory(Func<PackageBase, string, PackageLevelBase> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public PackageLevelBase Build(PackageBase package, string filePath)
            => _factory(package, filePath);

        public static IPackageLevelFactory Create(Func<PackageBase, string, PackageLevelBase> factory)
        {
            return new PackageLevelFactory(factory);
        }
    }
}
