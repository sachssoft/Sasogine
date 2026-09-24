using Sachssoft.Engine.Common;
using System;

namespace Sachssoft.Engine.Components.Models;

/// <summary>
/// Provides data for a definition property value change.
/// </summary>
public sealed class DefinitionPropertyChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefinitionPropertyChangedEventArgs"/> class.
    /// </summary>
    /// <param name="source">The definition whose property value changed.</param>
    /// <param name="oldValue">The previous property value.</param>
    /// <param name="newValue">The new property value.</param>
    public DefinitionPropertyChangedEventArgs(IDefinition source, object? oldValue, object? newValue)
    {
        ArgumentNullException.ThrowIfNull(source);

        Source = source;
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>
    /// Gets the definition whose property value changed.
    /// </summary>
    public IDefinition Source { get; }

    /// <summary>
    /// Gets the previous property value.
    /// </summary>
    public object? OldValue { get; }

    /// <summary>
    /// Gets the new property value.
    /// </summary>
    public object? NewValue { get; }
}