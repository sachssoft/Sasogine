namespace Sachssoft.Engine.Resources.Localization;

/// <summary>Represents a localized asset entry with an invariant resource source.</summary>
/// <param name="Key">The localization key.</param>
/// <param name="Fallback">The concrete source for the invariant asset.</param>
public readonly record struct LocalizedAssetEntry(
    string Key,
    ResourceSourceBase Fallback) : ILocalizedEntry;
