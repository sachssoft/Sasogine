using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Components
{
    public interface IComponentProvider
    {
        bool TryGetComponent<T>([MaybeNullWhen(false)] out T component) where T : class, IComponent;

    }
}
