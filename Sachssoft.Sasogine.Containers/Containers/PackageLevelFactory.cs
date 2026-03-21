using Sachssoft.Sasofly.Documents.Naming;
using System;

namespace Sachssoft.Sasogine.Containers
{
    public sealed class PackageLevelFactory : IPackageLevelFactory
    {
        private readonly Func<PackageBase, string, PackageLevelBase> _factory;
        private readonly INamingConvention? _namingConvention;

        public INamingConvention? NamingConvention => _namingConvention;

        private PackageLevelFactory(Func<PackageBase, string, PackageLevelBase> factory, INamingConvention? namingConvention = null)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _namingConvention = namingConvention;
        }

        public PackageLevelBase Build(PackageBase package, string filePath)
            => _factory(package, filePath);

        public static IPackageLevelFactory Create(Func<PackageBase, string, PackageLevelBase> factory, INamingConvention? namingConvention = null)
        {
            return new PackageLevelFactory(factory, namingConvention);
        }
    }
}
