namespace Sachssoft.Engine.Resources.Localization;

/// <summary>Represents a localized text entry with an invariant fallback.</summary>
/// <param name="Key">The localization key.</param>
/// <param name="Fallback">The text used when no translation provides the key.</param>
public readonly record struct LocalizedTextEntry(
    string Key,
    string Fallback) : ILocalizedEntry;
