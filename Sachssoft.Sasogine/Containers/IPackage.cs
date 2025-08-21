using Sachssoft.Documents;
using Sachssoft.Documents.Json;
using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Containers
{
    public interface IPackage
    {
        public PackageManifest Manifest { get; }

        [AllowNull]
        public IDocumentFormatter ManifestFormat { get; set; }

        public PackageIcon Icon { get; }

        public PackagePreviews Previews { get; }

        bool IsReadOnly { get; }

        bool IsOpen { get; }

        IReadOnlyDictionary<string, IPackageAsset> Assets { get; }

        IReadOnlyCollection<PackageLevelBase> Levels { get; }

    }
}
