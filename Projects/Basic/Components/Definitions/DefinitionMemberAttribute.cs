using System;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Provides display metadata for a definition member.
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = false,
    Inherited = true)]
public sealed class DefinitionMemberAttribute : Attribute
{
    /// <summary>
    /// Gets the display title of the definition member.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the category used to group the definition member.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets the description of the definition member.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the display order of the definition member within its category.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Gets the visibility of the definition member.
    /// </summary>
    public DefinitionMemberVisibility Visibility { get; init; } =
        DefinitionMemberVisibility.Visible;
}