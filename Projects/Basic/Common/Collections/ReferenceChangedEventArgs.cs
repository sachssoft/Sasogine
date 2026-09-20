using System;

namespace Sachssoft.Sasogine.Common.Collections;

/// <summary>
/// Provides data for a reference change within a referencable collection.
/// </summary>
/// <typeparam name="T">
/// The type of referenced item.
/// </typeparam>
public sealed class ReferenceChangedEventArgs<T> : EventArgs
    where T : class, IEngineReferenceable
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReferenceChangedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="item">The item affected by the reference change.</param>
    public ReferenceChangedEventArgs(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Item = item;
    }

    /// <summary>
    /// Gets the item affected by the reference change.
    /// </summary>
    public T Item { get; }
}