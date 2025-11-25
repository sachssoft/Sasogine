using Sachssoft.Inspection;
using Sachssoft.Sasogine.Inspection;
using System;

namespace Sachssoft.Sasogine.Assets;

public interface IAsset : IIdentifiable
{     
    IAssetSource? Source { get; set; }

    bool IsError { get; }

    object? Instance { get; }
}
