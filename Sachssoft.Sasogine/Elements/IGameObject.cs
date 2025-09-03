using Sachssoft.Observables;
using System;

namespace Sachssoft.Sasogine.Elements;

[Obsolete("Use Runtime Component Or NotifyingObject")]
public interface IGameObject : IIdentifiable, INamed, IClassifiable
{
    object? DataContext { get; set; }
}
