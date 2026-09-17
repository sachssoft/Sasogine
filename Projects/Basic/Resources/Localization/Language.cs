using System;

namespace Sachssoft.Sasogine.Resources.Localization;

/// <summary>
/// Represents a language and its pluralization rule.
/// </summary>
public sealed class Language
{
    private readonly Func<int, LocalizationPluralCase> _pluralSelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class.
    /// </summary>
    /// <param name="name">
    /// The language code, such as <c>de</c>, <c>en</c>, or <c>ru</c>.
    /// </param>
    /// <param name="pluralSelector">
    /// The function used to determine the plural case for a quantity.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty,
    /// or consists only of white-space characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pluralSelector"/> is <see langword="null"/>.
    /// </exception>
    public Language(
        string name,
        Func<int, LocalizationPluralCase> pluralSelector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(pluralSelector);

        Name = name;
        _pluralSelector = pluralSelector;
    }

    /// <summary>
    /// Gets the language code.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the plural case for the specified quantity.
    /// </summary>
    /// <param name="quantity">
    /// The quantity for which to determine the plural case.
    /// </param>
    /// <returns>
    /// The plural case corresponding to the specified quantity.
    /// </returns>
    public LocalizationPluralCase GetPluralCase(int quantity)
    {
        return _pluralSelector(quantity);
    }

    /// <summary>
    /// Returns the language code.
    /// </summary>
    /// <returns>
    /// The language code.
    /// </returns>
    public override string ToString()
    {
        return Name;
    }
}