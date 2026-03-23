using System;

namespace Sachssoft.Sasogine.Containers
{
    public class PackageLevelBase : PackageEntryBase
    {
        public PackageLevelBase(PackageBase package, string filePath) : base(package, filePath)
        {
        }

        protected override sealed string RootPath => ProjectedPackageLevelCollection.FILE_PATH;

        public int Index { get; set; }

        public Guid Guid { get; set; } = Guid.NewGuid();

        public string? Title { get; set; }

        public string? Description { get; set; }
    }
}
