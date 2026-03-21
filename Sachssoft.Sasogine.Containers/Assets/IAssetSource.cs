using Sachssoft.Sasofly.Inspection;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    public interface IAssetSource : IHasGuid, IIdentifiable
    {
        event EventHandler? AssetChanged;

        string FileName { get; set; }

        AssetCategory Category { get; set; }

        string? CategoryName { get; set; }

        IAsset? Asset { get; }

        Stream Open();
    }
}
