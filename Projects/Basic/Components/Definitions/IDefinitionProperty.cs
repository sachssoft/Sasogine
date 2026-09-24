using Sachssoft.Engine;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Represents a property of a definition and provides access to its metadata and value.
/// </summary>
/// <remarks>
/// Provides a common abstraction for definition properties independent of whether
/// their metadata and value access are provided explicitly or through reflection.
/// </remarks>
public interface IDefinitionProperty
{
    /// <summary>
    /// Occurs when the property value has been changed through this property.
    /// </summary>
    event EventHandler<DefinitionPropertyChangedEventArgs>? ValueChanged;

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the value type of the property.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Gets a value indicating whether the property is read-only.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Gets the attributes associated with the property.
    /// </summary>
    IReadOnlyList<Attribute> Attributes { get; }

    /// <summary>
    /// Gets the value of the property from the specified definition.
    /// </summary>
    /// <param name="source">The definition from which to retrieve the value.</param>
    /// <returns>The current property value.</returns>
    object? GetValue(IDefinition source);

    /// <summary>
    /// Determines whether the specified value can be assigned to the property.
    /// </summary>
    /// <param name="source">The definition on which the value would be assigned.</param>
    /// <param name="value">The value to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the value can be assigned; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    bool CanSetValue(IDefinition source, object? value);

    /// <summary>
    /// Sets the value of the property on the specified definition.
    /// </summary>
    /// <param name="source">The definition on which to set the value.</param>
    /// <param name="value">The value to assign.</param>
    void SetValue(IDefinition source, object? value);

    /// <summary>
    /// Gets the first attribute of the specified type associated with the property.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute to retrieve.</typeparam>
    /// <returns>
    /// The first matching attribute, or <see langword="null"/> if no matching attribute exists.
    /// </returns>
    TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute;

    /// <summary>
    /// Determines whether an attribute of the specified type is associated with the property.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute to check.</typeparam>
    /// <returns>
    /// <see langword="true"/> if a matching attribute exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    bool HasAttribute<TAttribute>() where TAttribute : Attribute;
}