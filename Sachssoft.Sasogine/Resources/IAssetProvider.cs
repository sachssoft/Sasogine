using System;
using System.ComponentModel;
using Sachssoft.Sasogine.Elements;

namespace Sachssoft.Sasogine.Resources;

public interface IAssetProvider : IGameObject, INotifyPropertyChanged
{
    object? GetAsset();

    bool IsError { get; }
}
