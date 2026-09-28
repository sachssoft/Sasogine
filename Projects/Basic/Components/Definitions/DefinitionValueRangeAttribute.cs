using System;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Defines the allowed value range for a definition member.
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = true,
    Inherited = true)]
public sealed class DefinitionValueRangeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefinitionValueRangeAttribute"/> class.
    /// </summary>
    /// <param name="minimum">The minimum allowed value represented as text.</param>
    /// <param name="maximum">The maximum allowed value represented as text.</param>
    public DefinitionValueRangeAttribute(string minimum, string maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefinitionValueRangeAttribute"/> class.
    /// </summary>
    /// <param name="fieldName">The field name of a composite value.</param>
    /// <param name="minimum">The minimum allowed value represented as text.</param>
    /// <param name="maximum">The maximum allowed value represented as text.</param>
    public DefinitionValueRangeAttribute(string fieldName, string minimum, string maximum)
    {
        FieldName = fieldName;
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Gets the minimum allowed value represented as text.
    /// </summary>
    public string Minimum { get; }

    /// <summary>
    /// Gets the maximum allowed value represented as text.
    /// </summary>
    public string Maximum { get; }

    /// <summary>
    /// Gets the optional field name for a composite value.
    /// </summary>
    public string? FieldName { get; }
}