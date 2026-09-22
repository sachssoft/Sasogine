using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Resources.Localization;

/// <summary>
/// Stores localized string values and their plural variants.
/// </summary>
public class LocalizedDictionary
{
    private readonly Dictionary<string, Entry> _entries =
        new(StringComparer.OrdinalIgnoreCase);

    private bool _isClosed;

    private sealed class Entry
    {
        public string? Value { get; init; }

        public IReadOnlyDictionary<LocalizationPluralCase, string?>? PluralCases
        {
            get;
            init;
        }
    }

    /// <summary>
    /// Gets the number of localized entries.
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Gets a value indicating whether this dictionary is closed for modifications.
    /// </summary>
    public bool IsClosed => _isClosed;

    /// <summary>
    /// Determines whether the specified key exists.
    /// </summary>
    public bool ContainsKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _entries.ContainsKey(key);
    }

    /// <summary>
    /// Gets the localized string associated with the specified key.
    /// </summary>
    public string? GetValue(string key, string? defaultValue = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        return _entries.TryGetValue(key, out Entry? entry)
            ? entry.Value ?? defaultValue
            : defaultValue;
    }

    /// <summary>
    /// Gets the localized string associated with the specified key and quantity.
    /// </summary>
    public string? GetValue(
        string key,
        int quantity,
        string? defaultValue = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (!_entries.TryGetValue(key, out Entry? entry))
            return defaultValue;

        if (entry.PluralCases is null || entry.PluralCases.Count == 0)
            return entry.Value ?? defaultValue;

        LocalizationPluralCase pluralCase =
            ResolvePluralCase(entry.PluralCases, quantity);

        if (entry.PluralCases.TryGetValue(pluralCase, out string? value))
            return value ?? entry.Value ?? defaultValue;

        if (entry.PluralCases.TryGetValue(
            LocalizationPluralCase.Default,
            out value))
        {
            return value ?? entry.Value ?? defaultValue;
        }

        return entry.Value ?? defaultValue;
    }

    /// <summary>
    /// Attempts to get the localized string associated with the specified key.
    /// </summary>
    public bool TryGetValue(string key, out string? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (_entries.TryGetValue(key, out Entry? entry) &&
            entry.Value is not null)
        {
            value = entry.Value;
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Attempts to get the localized string associated with the specified key and quantity.
    /// </summary>
    public bool TryGetValue(string key, int quantity, out string? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (!_entries.ContainsKey(key))
        {
            value = null;
            return false;
        }

        value = GetValue(key, quantity);
        return value is not null;
    }

    /// <summary>
    /// Adds or replaces a localized string entry.
    /// </summary>
    protected void AddEntry(string key, string? value)
    {
        EnsureWritable();
        ArgumentException.ThrowIfNullOrEmpty(key);

        _entries[key] = new Entry
        {
            Value = value
        };
    }

    /// <summary>
    /// Adds or replaces a localized string entry with plural variants.
    /// </summary>
    protected void AddEntry(
        string key,
        string? value,
        IReadOnlyDictionary<LocalizationPluralCase, string?> pluralCases)
    {
        EnsureWritable();
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(pluralCases);

        _entries[key] = new Entry
        {
            Value = value,
            PluralCases = pluralCases
        };
    }

    /// <summary>
    /// Prevents further modifications to this dictionary.
    /// </summary>
    public void Close()
    {
        _isClosed = true;
    }

    private void EnsureWritable()
    {
        if (_isClosed)
            throw new InvalidOperationException(
                "Cannot modify a closed localized dictionary.");
    }

    private static LocalizationPluralCase ResolvePluralCase(
        IReadOnlyDictionary<LocalizationPluralCase, string?> pluralCases,
        int quantity)
    {
        return quantity switch
        {
            0 when pluralCases.ContainsKey(LocalizationPluralCase.Zero) =>
                LocalizationPluralCase.Zero,
            1 when pluralCases.ContainsKey(LocalizationPluralCase.One) =>
                LocalizationPluralCase.One,
            2 when pluralCases.ContainsKey(LocalizationPluralCase.Two) =>
                LocalizationPluralCase.Two,
            >= 3 and <= 4 when pluralCases.ContainsKey(LocalizationPluralCase.Few) =>
                LocalizationPluralCase.Few,
            >= 5 when pluralCases.ContainsKey(LocalizationPluralCase.Many) =>
                LocalizationPluralCase.Many,
            _ => LocalizationPluralCase.Default
        };
    }
}
