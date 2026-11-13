using Sachssoft.Engine.Assets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine.Resources.Localization;

/// <summary>
/// Manages localized strings and assets for the active language.
/// </summary>
public sealed class LocalizationManager
{
    private readonly Dictionary<string, LanguageEntry> _languages =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly LocalizedDictionary _emptyDictionary = new();

    private Language? _currentLanguage;
    private bool _isClosed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationManager"/> class.
    /// </summary>
    /// <param name="fallbackLanguage">
    /// The fallback language, or <see langword="null"/> to use English.
    /// </param>
    public LocalizationManager(Language? fallbackLanguage = null)
    {
        FallbackLanguage = fallbackLanguage ?? Languages.English;
    }

    /// <summary>
    /// Occurs when the current localization language changes.
    /// </summary>
    public event EventHandler? LanguageChanged;

    /// <summary>
    /// Gets the fallback language used when localization data is unavailable
    /// for the current language.
    /// </summary>
    public Language FallbackLanguage { get; }

    /// <summary>
    /// Gets or sets the current language used for localization.
    /// </summary>
    /// <remarks>
    /// If no language has been explicitly assigned, the fallback language
    /// is returned.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Language CurrentLanguage
    {
        get => _currentLanguage ?? FallbackLanguage;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(CurrentLanguage, value))
                return;

            _currentLanguage = value;
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the localization manager
    /// has been closed for further modification.
    /// </summary>
    public bool IsClosed => _isClosed;

    /// <summary>
    /// Gets the localized string dictionary for the current language.
    /// </summary>
    /// <remarks>
    /// If no dictionary exists for the current language, the fallback language
    /// dictionary is returned. If neither exists, an empty dictionary is returned.
    /// </remarks>
    public LocalizedDictionary Entries
    {
        get
        {
            if (TryGetEntry(CurrentLanguage, out LanguageEntry? entry))
                return entry.Dictionary;

            if (!ReferenceEquals(CurrentLanguage, FallbackLanguage) &&
                TryGetEntry(FallbackLanguage, out LanguageEntry? fallback))
            {
                return fallback.Dictionary;
            }

            return _emptyDictionary;
        }
    }

    /// <summary>
    /// Gets the localized asset store for the current language.
    /// </summary>
    /// <remarks>
    /// If no asset store exists for the current language, the fallback language
    /// asset store is returned.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// No localized assets are registered for either the current language
    /// or the fallback language.
    /// </exception>
    public AssetStore Assets
    {
        get
        {
            if (TryGetEntry(CurrentLanguage, out LanguageEntry? entry))
                return entry.Assets;

            if (!ReferenceEquals(CurrentLanguage, FallbackLanguage) &&
                TryGetEntry(FallbackLanguage, out LanguageEntry? fallback))
            {
                return fallback.Assets;
            }

            throw new InvalidOperationException(
                $"No localized assets are registered for the current language " +
                $"'{CurrentLanguage.Name}' or fallback language '{FallbackLanguage.Name}'.");
        }
    }

    /// <summary>
    /// Resets the current language to the fallback language.
    /// </summary>
    public void ResetLanguage()
    {
        if (_currentLanguage is null)
            return;

        var previousLanguage = _currentLanguage;
        _currentLanguage = null;

        if (!ReferenceEquals(previousLanguage, FallbackLanguage))
            LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Adds localization data for the specified language.
    /// </summary>
    /// <param name="entry">
    /// The language entry to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="entry"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The localization manager has already been closed.
    /// </exception>
    public void Add(LanguageEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (_isClosed)
        {
            throw new InvalidOperationException(
                "The localization manager is closed.");
        }

        _languages[entry.Language.Name] = entry;
    }

    /// <summary>
    /// Determines whether localization data exists for the specified language.
    /// </summary>
    /// <param name="language">
    /// The language to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if localization data is registered for the
    /// specified language; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Language language)
    {
        ArgumentNullException.ThrowIfNull(language);

        return _languages.ContainsKey(language.Name);
    }

    /// <summary>
    /// Attempts to get the localization entry for the specified language.
    /// </summary>
    /// <param name="language">
    /// The language whose entry should be retrieved.
    /// </param>
    /// <param name="entry">
    /// When this method returns, contains the registered language entry
    /// if found; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the language entry was found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetEntry(
        Language language,
        [NotNullWhen(true)] out LanguageEntry? entry)
    {
        ArgumentNullException.ThrowIfNull(language);

        return _languages.TryGetValue(language.Name, out entry);
    }

    /// <summary>
    /// Gets a localized string, falling back per key when the active translation is missing.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="defaultValue">The value used when neither language provides a translation.</param>
    /// <returns>The resolved translation or <paramref name="defaultValue"/>.</returns>
    public string? GetValue(string key, string? defaultValue = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        return (TryGetEntry(CurrentLanguage, out LanguageEntry? entry) ? entry.Dictionary.GetValue(key) : null)
            ?? (TryGetEntry(FallbackLanguage, out LanguageEntry? fallback) ? fallback.Dictionary.GetValue(key) : null)
            ?? defaultValue;
    }

    /// <summary>Gets the translated text for a typed entry or its invariant fallback.</summary>
    /// <param name="entry">The keyed text entry to resolve.</param>
    /// <returns>The resolved translation or the entry's invariant fallback.</returns>
    public string GetValue(LocalizedTextEntry entry) => GetValue(entry.Key) ?? entry.Fallback;

    /// <summary>
    /// Gets a pluralized string using the rules of the language that supplies the translation.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="quantity">The integer quantity used to select the plural form.</param>
    /// <param name="defaultValue">The value used when neither language provides a translation.</param>
    /// <returns>The resolved translation template or <paramref name="defaultValue"/>.</returns>
    public string? GetValue(string key, int quantity, string? defaultValue = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        return (TryGetEntry(CurrentLanguage, out LanguageEntry? entry)
                ? entry.Dictionary.GetValue(key, entry.GetPluralCase(quantity))
                : null)
            ?? (TryGetEntry(FallbackLanguage, out LanguageEntry? fallback)
                ? fallback.Dictionary.GetValue(key, fallback.GetPluralCase(quantity))
                : null)
            ?? defaultValue;
    }

    /// <summary>
    /// Closes registration and the registered dictionaries while keeping language selection available.
    /// </summary>
    internal void Close()
    {
        if (_isClosed)
            return;

        foreach (LanguageEntry entry in _languages.Values)
            entry.Dictionary.Close();

        _isClosed = true;
    }
}