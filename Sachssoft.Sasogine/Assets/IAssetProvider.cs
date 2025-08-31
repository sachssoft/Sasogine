using System;
using System.ComponentModel;
using Sachssoft.Sasogine.Elements;

namespace Sachssoft.Sasogine.Assets;

public interface IAssetProvider : IGameObject, INotifyPropertyChanged
{
    object? GetAsset();

    bool IsError { get; }
}
