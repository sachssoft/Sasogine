using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Provides extension methods for accessing common metadata associated with
/// <see cref="IDefinitionProperty"/> instances.
/// </summary>
public static class DefinitionPropertyExtensions
{
    /// <summary>
    /// Gets the category associated with the specified property.
    /// </summary>
    /// <param name="property">The definition property.</param>
    /// <returns>
    /// The category specified by <see cref="CategoryAttribute"/>, or
    /// <see langword="null"/> if no category is specified.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="property"/> is <see langword="null"/>.
    /// </exception>
    public static string? GetCategory(this IDefinitionProperty property)
    {
        ArgumentNullException.ThrowIfNull(property);

        return property.GetAttribute<CategoryAttribute>()?.Category;
    }

    /// <summary>
    /// Gets the display label associated with the specified property.
    /// </summary>
    /// <param name="property">The definition property.</param>
    /// <returns>
    /// The display name specified by <see cref="DisplayNameAttribute"/>, or the
    /// property name if no display name is specified.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="property"/> is <see langword="null"/>.
    /// </exception>
    public static string GetLabel(this IDefinitionProperty property)
    {
        ArgumentNullException.ThrowIfNull(property);

        return property.GetAttribute<DisplayNameAttribute>()?.DisplayName
            ?? property.Name;
    }

    /// <summary>
    /// Determines whether the specified property is browsable.
    /// </summary>
    /// <param name="property">The definition property.</param>
    /// <returns>
    /// <see langword="true"/> if the property is browsable or does not define
    /// a <see cref="BrowsableAttribute"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="property"/> is <see langword="null"/>.
    /// </exception>
    public static bool IsBrowsable(this IDefinitionProperty property)
    {
        ArgumentNullException.ThrowIfNull(property);

        return property.GetAttribute<BrowsableAttribute>()?.Browsable ?? true;
    }
}