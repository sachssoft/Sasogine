using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources;

public interface IResourceTreeEntry : IResourceEntry
{
    /// <summary>
    /// Kinder dieses Eintrags (immutable).
    /// </summary>
    IReadOnlySet<IResourceTreeEntry> Children { get; }

    /// <summary>
    /// Liefert ein direktes Kind nach Id oder null, wenn nicht gefunden.
    /// </summary>
    IResourceTreeEntry? GetChild(string id);

    /// <summary>
    /// Liefert alle direkten Kinder mit der angegebenen Klasse.
    /// </summary>
    IEnumerable<IResourceTreeEntry> GetChildrenByClass(string className);

    /// <summary>
    /// Liefert alle Nachkommen (rekursiv).
    /// </summary>
    IEnumerable<IResourceTreeEntry> GetDescendants();
}
