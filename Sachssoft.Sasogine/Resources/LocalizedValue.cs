using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Represents a multilingual value with versions for multiple languages.
/// Example: "en" = "Starter", "de" = "Starter", or any objects for different languages.
/// </summary>
/// <typeparam name="T">The type of the stored value, e.g., string, Texture2D, etc.</typeparam>
public class LocalizedValue<T>
{
    private readonly Dictionary<string, T?> _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedValue{T}"/> class.
    /// </summary>
    public LocalizedValue()
    {
        _values = new();
    }

    /// <summary>
    /// Creates a new instance of <see cref="LocalizedValue{T}"/>.
    /// </summary>
    /// <returns>The new instance.</returns>
    public static LocalizedValue<T> Create() => new LocalizedValue<T>();

    /// <summary>
    /// Adds or updates a value for a specific language code.
    /// </summary>
    /// <param name="language_token">The language code, e.g. "en" for English.</param>
    /// <param name="value">The value for the language.</param>
    /// <returns>The instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if the language code is invalid.</exception>
    public LocalizedValue<T> Add(string language_token, T? value)
    {
        if (!IsValidLanguageToken(language_token))
            throw new ArgumentException($"'{language_token}' is not a valid language code.");

        _values[language_token] = value;
        return this;
    }

    /// <summary>
    /// Adds or updates a value for a language based on a <see cref="CultureInfo"/>.
    /// </summary>
    /// <param name="culture">The <see cref="CultureInfo"/> object representing the language.</param>
    /// <param name="value">The value for the language.</param>
    /// <returns>The instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="culture"/> is null.</exception>
    public LocalizedValue<T> Add(CultureInfo culture, T? value)
    {
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        return Add(culture.Name, value);
    }

    /// <summary>
    /// Attempts to add a value for the specified language code.
    /// If the language code is invalid, nothing is added and a Debug.Assert is triggered.
    /// </summary>
    /// <param name="language_token">The language code.</param>
    /// <param name="value">The value for the language.</param>
    /// <returns>True if added; false if the language code is invalid.</returns>
    public bool TryAdd(string? language_token, T? value)
    {
        if (!IsValidLanguageToken(language_token))
        {
            Debug.Assert(false, $"Invalid language token ignored: '{language_token}'");
            return false;
        }

        _values[language_token] = value;
        return true;
    }

    /// <summary>
    /// Gets the value for the specified language code.
    /// If no value for the language is found, it returns the value for "en" (English) as a fallback.
    /// If that is also not present, returns the <paramref name="default_value"/>.
    /// </summary>
    /// <param name="language_token">The language code, e.g. "de", "en-US".</param>
    /// <param name="default_value">The value to return if no matching value is found.</param>
    /// <returns>The value for the language or the default value.</returns>
    public T? GetValue(string? language_token, T? default_value = default)
    {
        if (_values.TryGetValue(language_token ?? string.Empty, out var val))
            return val;

        if (_values.TryGetValue("en", out var fallback))
            return fallback;

        return default_value;
    }

    /// <summary>
    /// Gets all entries as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, T?> Entries => _values;

    /// <summary>
    /// Checks if a language code is valid (corresponds to a known culture).
    /// </summary>
    /// <param name="language_token">The language code.</param>
    /// <returns>True if the language code is valid; otherwise, false.</returns>
    private static bool IsValidLanguageToken(string language_token)
    {
        if (string.IsNullOrWhiteSpace(language_token))
            return false;

        try
        {
            CultureInfo.GetCultureInfo(language_token);
            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }
}
