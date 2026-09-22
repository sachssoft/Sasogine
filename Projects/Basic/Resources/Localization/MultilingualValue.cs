using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Resources.Localization;

/// <summary>
/// Represents an immutable value that can provide different values
/// depending on the requested language.
/// </summary>
/// <typeparam name="T">The type of the multilingual value.</typeparam>
public sealed class MultilingualValue<T>
{
    private readonly Dictionary<string, T> _values;
    private readonly T? _fallback;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultilingualValue{T}"/> class.
    /// </summary>
    /// <param name="values">The initial language-specific values.</param>
    /// <param name="fallback">
    /// The value returned when no value exists for the requested language.
    /// </param>
    public MultilingualValue(
        IDictionary<Language, T>? values = null,
        T? fallback = default)
    {
        _values = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        if (values is not null)
        {
            foreach (KeyValuePair<Language, T> pair in values)
            {
                ArgumentNullException.ThrowIfNull(pair.Key);
                _values[pair.Key.Name] = pair.Value;
            }
        }

        _fallback = fallback;
    }

    private MultilingualValue(Dictionary<string, T> values, T? fallback)
    {
        _values = values;
        _fallback = fallback;
    }

    /// <summary>
    /// Gets the fallback value used when no language-specific value exists.
    /// </summary>
    public T? Fallback => _fallback;

    /// <summary>
    /// Gets the explicitly defined language-specific values.
    /// </summary>
    public IReadOnlyDictionary<string, T> Values => _values;

    /// <summary>
    /// Gets the language names for which values are explicitly defined.
    /// </summary>
    public IEnumerable<string> Languages => _values.Keys;

    /// <summary>
    /// Gets the number of explicitly defined language-specific values.
    /// </summary>
    public int Count => _values.Count;

    /// <summary>
    /// Creates a new instance containing the specified value for a language.
    /// </summary>
    /// <param name="language">The language to associate with the value.</param>
    /// <param name="value">The value associated with the language.</param>
    /// <returns>A new instance containing the specified language and value.</returns>
    public MultilingualValue<T> With(Language language, T value)
    {
        ArgumentNullException.ThrowIfNull(language);

        var copy = new Dictionary<string, T>(_values, StringComparer.OrdinalIgnoreCase)
        {
            [language.Name] = value
        };

        return new MultilingualValue<T>(copy, _fallback);
    }

    /// <summary>
    /// Creates a new instance without the value associated with the specified language.
    /// </summary>
    /// <param name="language">The language to remove.</param>
    /// <returns>A new instance without the specified language.</returns>
    public MultilingualValue<T> Without(Language language)
    {
        ArgumentNullException.ThrowIfNull(language);

        var copy = new Dictionary<string, T>(_values, StringComparer.OrdinalIgnoreCase);
        copy.Remove(language.Name);

        return new MultilingualValue<T>(copy, _fallback);
    }

    /// <summary>
    /// Gets the value associated with the specified language.
    /// </summary>
    /// <param name="language">The language whose value should be retrieved.</param>
    /// <returns>
    /// The language-specific value when available; otherwise the fallback value.
    /// </returns>
    public T? Get(Language language)
    {
        ArgumentNullException.ThrowIfNull(language);
        return Get(language.Name);
    }

    /// <summary>
    /// Gets the value associated with the specified language name.
    /// </summary>
    /// <param name="language">
    /// The language name or code whose value should be retrieved.
    /// </param>
    /// <returns>
    /// The language-specific value when available; otherwise the fallback value.
    /// </returns>
    public T? Get(string language)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        return _values.TryGetValue(language, out T? value)
            ? value
            : _fallback;
    }

    /// <summary>
    /// Attempts to get a value explicitly associated with the specified language.
    /// </summary>
    /// <param name="language">The language whose value should be retrieved.</param>
    /// <param name="value">
    /// When this method returns, contains the language-specific value when found;
    /// otherwise the fallback value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a language-specific value exists;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool TryGet(Language language, out T? value)
    {
        ArgumentNullException.ThrowIfNull(language);
        return TryGet(language.Name, out value);
    }

    /// <summary>
    /// Attempts to get a value explicitly associated with the specified language name.
    /// </summary>
    /// <param name="language">
    /// The language name or code whose value should be retrieved.
    /// </param>
    /// <param name="value">
    /// When this method returns, contains the language-specific value when found;
    /// otherwise the fallback value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a language-specific value exists;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool TryGet(string language, out T? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        if (_values.TryGetValue(language, out T? languageValue))
        {
            value = languageValue;
            return true;
        }

        value = _fallback;
        return false;
    }

    /// <summary>
    /// Determines whether a value is explicitly defined for the specified language.
    /// </summary>
    /// <param name="language">The language to check.</param>
    /// <returns>
    /// <see langword="true"/> if a value is explicitly defined;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool Contains(Language language)
    {
        ArgumentNullException.ThrowIfNull(language);
        return _values.ContainsKey(language.Name);
    }
}