using Sachssoft.Observables;
using Sachssoft.Sasogine.Elements;
using System;

namespace Sachssoft.Sasogine.Assets;

public interface IAsset : IIdentifiable, IGameObjectElement
{     
    IAssetSource Source { get; }

    bool IsError { get; }
}
