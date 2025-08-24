using Sachssoft.Observables;

namespace Sachssoft.Sasogine.Elements;

public interface IGameObject : IIdentifiable, INamed, IClassifiable
{
    object? DataContext { get; set; }
}
