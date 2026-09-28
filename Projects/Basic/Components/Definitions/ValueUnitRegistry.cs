using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Provides access to registered value units.
/// </summary>
public static class ValueUnitRegistry
{
    private static readonly Dictionary<string, ValueUnit> Units =
        new(StringComparer.Ordinal);

    static ValueUnitRegistry()
    {
        Register(ValueUnits.Pixel);

        Register(ValueUnits.Millimeter);
        Register(ValueUnits.Centimeter);
        Register(ValueUnits.Meter);
        Register(ValueUnits.Kilometer);

        Register(ValueUnits.Radian);
        Register(ValueUnits.Degree);

        Register(ValueUnits.Millisecond);
        Register(ValueUnits.Second);
        Register(ValueUnits.Minute);
        Register(ValueUnits.Hour);

        Register(ValueUnits.Hertz);
        Register(ValueUnits.Kilohertz);
        Register(ValueUnits.Megahertz);

        Register(ValueUnits.MeterPerSecond);
        Register(ValueUnits.KilometerPerHour);

        Register(ValueUnits.MeterPerSecondSquared);

        Register(ValueUnits.Gram);
        Register(ValueUnits.Kilogram);
        Register(ValueUnits.Tonne);

        Register(ValueUnits.Ratio);
        Register(ValueUnits.Percent);
    }

    /// <summary>
    /// Gets all registered value units.
    /// </summary>
    public static IReadOnlyCollection<ValueUnit> Values => Units.Values;

    /// <summary>
    /// Registers the specified value unit.
    /// </summary>
    /// <param name="unit">The value unit to register.</param>
    /// <exception cref="InvalidOperationException">
    /// A value unit with the same name is already registered.
    /// </exception>
    public static void Register(ValueUnit unit)
    {
        if (!Units.TryAdd(unit.Name, unit))
        {
            throw new InvalidOperationException(
                $"A value unit with the name '{unit.Name}' is already registered.");
        }
    }

    /// <summary>
    /// Gets the value unit with the specified name.
    /// </summary>
    /// <param name="name">The name of the value unit.</param>
    /// <returns>The registered value unit.</returns>
    /// <exception cref="KeyNotFoundException">
    /// No value unit with the specified name is registered.
    /// </exception>
    public static ValueUnit Get(string name)
    {
        if (Units.TryGetValue(name, out var unit))
            return unit;

        throw new KeyNotFoundException(
            $"No value unit with the name '{name}' is registered.");
    }

    /// <summary>
    /// Gets all registered value units that belong to the specified group.
    /// </summary>
    /// <param name="group">The value unit group.</param>
    /// <returns>
    /// The registered value units that belong to the specified group.
    /// </returns>
    public static IEnumerable<ValueUnit> GetUnits(ValueUnitGroup group)
        => Units.Values.Where(unit => unit.Group == group);

    /// <summary>
    /// Attempts to get the value unit with the specified name.
    /// </summary>
    /// <param name="name">The name of the value unit.</param>
    /// <param name="unit">The registered value unit, if found.</param>
    /// <returns>
    /// <see langword="true"/> if the value unit was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryGet(string name, out ValueUnit unit)
        => Units.TryGetValue(name, out unit);
}