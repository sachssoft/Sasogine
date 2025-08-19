using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;

namespace Sachssoft.Sasogine.Containers
{
    public class PublishedPackage : PackageBase
    {
        public override IReadOnlyCollection<PackageLevelBase> Levels => throw new NotImplementedException();

        public PublishedPackage(Stream stream)
            : base(stream ?? throw new ArgumentNullException(nameof(stream)), true)
        {
        }

        public PublishedPackage(string filePath)
            : base(File.Open(filePath ?? throw new ArgumentNullException(nameof(filePath)), FileMode.Open, FileAccess.Read), true)
        {
        }

        public PublishedPackage(Assembly assembly, string resourceName)
            : base(assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Resource {resourceName} nicht gefunden."), true)
        {
        }

        public PublishedPackage(string isolatedStorageFilePath, IsolatedStorageFile storage)
            : base(storage.OpenFile(isolatedStorageFilePath, FileMode.Open, FileAccess.Read), true)
        {
        }

        public PublishedPackage(byte[] data)
            : base(new MemoryStream(data ?? throw new ArgumentNullException(nameof(data))), true)
        {
        }

        public override IPackageAsset? GetAsset(string filePath)
        {
            throw new NotImplementedException();
        }

        //public string? Password { get; set; }
    }
}
