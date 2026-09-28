using System;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Represents a value unit within a compatible unit group.
/// </summary>
public readonly struct ValueUnit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueUnit"/> struct.
    /// </summary>
    /// <param name="name">The unique name of the unit.</param>
    /// <param name="symbol">The display symbol of the unit.</param>
    /// <param name="group">The group to which the unit belongs.</param>
    /// <param name="factor">The conversion factor relative to the base unit.</param>
    public ValueUnit(
        string name,
        string symbol,
        ValueUnitGroup group,
        double factor)
    {
        Name = name;
        Symbol = symbol;
        Group = group;
        Factor = factor;
    }

    /// <summary>
    /// Gets the unique name of the unit.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the display symbol of the unit.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// Gets the group to which the unit belongs.
    /// </summary>
    public ValueUnitGroup Group { get; }

    /// <summary>
    /// Gets the conversion factor relative to the base unit.
    /// </summary>
    public double Factor { get; }

    /// <summary>
    /// Converts a value from this unit to the specified unit.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="unit">The target unit.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="InvalidOperationException">
    /// The units belong to different groups.
    /// </exception>
    public double ToUnit(double value, ValueUnit unit)
    {
        if (Group != unit.Group)
            throw new InvalidOperationException(
                $"Cannot convert '{Name}' to '{unit.Name}'.");

        return value * Factor / unit.Factor;
    }
}