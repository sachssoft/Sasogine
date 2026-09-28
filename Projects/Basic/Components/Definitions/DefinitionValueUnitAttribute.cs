using System;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Specifies the value units supported by a definition member.
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = false,
    Inherited = true)]
public sealed class DefinitionValueUnitAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefinitionValueUnitAttribute"/> class.
    /// </summary>
    /// <param name="unitNames">
    /// The names of the supported value units.
    /// </param>
    /// <exception cref="ArgumentException">
    /// A specified unit name is not registered.
    /// </exception>
    public DefinitionValueUnitAttribute(params string[] unitNames)
    {
        ArgumentNullException.ThrowIfNull(unitNames);

        foreach (var unitName in unitNames)
        {
            if (!ValueUnitRegistry.TryGet(unitName, out _))
                throw new ArgumentException(
                    $"The value unit '{unitName}' is not registered.",
                    nameof(unitNames));
        }

        UnitNames = unitNames;
    }

    /// <summary>
    /// Gets the names of the supported value units.
    /// </summary>
    public string[] UnitNames { get; }

    /// <summary>
    /// Resolves the specified unit name to its registered value unit.
    /// </summary>
    /// <param name="unitName">The name of the unit to resolve.</param>
    /// <returns>The registered value unit.</returns>
    /// <exception cref="ArgumentException">
    /// The specified unit is not supported by this attribute.
    /// </exception>
    public ValueUnit ToUnit(string unitName)
    {
        if (!ValueUnitRegistry.TryGet(unitName, out var unit))
            throw new ArgumentException(
                $"The value unit '{unitName}' is not supported.",
                nameof(unitName));

        return unit;
    }
}