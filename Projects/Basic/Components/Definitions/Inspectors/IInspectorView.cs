using System;

namespace Sachssoft.Engine.Components.Definitions.Inspectors;

/// <summary>
/// Describes an inspector's fields independently of a UI framework.
/// </summary>
public interface IInspectorView
{
    /// <summary>
    /// Gets or creates a group with a stable name and display title.
    /// </summary>
    IInspectorView Group(string name, string title);

    /// <summary>
    /// Adds an explicitly bound property.
    /// A missing setter makes the field read-only.
    /// </summary>
    /// <typeparam name="TValue">The property value type.</typeparam>
    /// <param name="name">The stable property name.</param>
    /// <param name="title">The displayed title.</param>
    /// <param name="read">Reads the current value.</param>
    /// <param name="write">Writes a value, or null for read-only access.</param>
    void Add<TValue>(
        string name,
        string title,
        Func<TValue> read,
        Action<TValue>? write = null);
}