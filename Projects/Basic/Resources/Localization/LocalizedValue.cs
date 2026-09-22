using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine.Resources.Localization;

/// <summary>
/// Represents a reference to a localized string with an optional fallback value.
/// </summary>
public record LocalizedValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedValue"/> record.
    /// </summary>
    /// <param name="key">
    /// The localization key.
    /// </param>
    /// <param name="fallback">
    /// The optional fallback value.
    /// </param>
    [SetsRequiredMembers]
    public LocalizedValue(
        string key,
        string? fallback = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        Key = key;
        Fallback = fallback;
    }

    /// <summary>
    /// Gets the localization key.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the optional fallback value.
    /// </summary>
    public string? Fallback { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Fallback is null
            ? $"{Key} => null"
            : $"{Key} => {Fallback}";
    }
}