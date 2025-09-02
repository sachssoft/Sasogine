using Sachssoft.Observables;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    public interface IAssetSource : IHasGuid
    {
        string FileName { get; set; }

        AssetCategory Category { get; set; }

        string? CategoryName { get; set; }

        IAsset? Asset { get; }

        Stream Open();
    }
}
