using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Components;

/// <summary>
/// Defines a provider that can retrieve components by type.
/// </summary>
public interface IComponentProvider
{
    /// <summary>
    /// Attempts to retrieve a component of the specified type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of component to retrieve.
    /// </typeparam>
    /// <param name="component">
    /// When this method returns <see langword="true"/>, contains the component
    /// of the requested type; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a component of the specified type was found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGetComponent<T>(
        [MaybeNullWhen(false)] out T component)
        where T : class, IComponent;
}