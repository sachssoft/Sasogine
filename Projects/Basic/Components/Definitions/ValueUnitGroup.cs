using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Represents a group of compatible value units.
/// </summary>
public readonly struct ValueUnitGroup : IEquatable<ValueUnitGroup>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueUnitGroup"/> struct.
    /// </summary>
    /// <param name="name">The unique name of the unit group.</param>
    public ValueUnitGroup(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    /// <summary>
    /// Gets the unique name of the unit group.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets all registered value units that belong to this group.
    /// </summary>
    /// <returns>
    /// An enumerable collection containing the registered value units
    /// that belong to this group.
    /// </returns>
    public IEnumerable<ValueUnit> GetUnits()
        => ValueUnitRegistry.GetUnits(this);

    /// <inheritdoc/>
    public bool Equals(ValueUnitGroup other)
        => StringComparer.Ordinal.Equals(Name, other.Name);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is ValueUnitGroup other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(Name);

    /// <inheritdoc/>
    public override string ToString()
        => Name;

    /// <inheritdoc/>
    public static bool operator ==(ValueUnitGroup left, ValueUnitGroup right)
        => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ValueUnitGroup left, ValueUnitGroup right)
        => !left.Equals(right);
}