using System;
using System.ComponentModel;
using sachssoft.Sasogine.Elements;

namespace sachssoft.Sasogine.Assets;

public interface IAssetProvider : IGameObject, INotifyPropertyChanged
{
    object? GetAsset();

    bool IsError { get; }
}
