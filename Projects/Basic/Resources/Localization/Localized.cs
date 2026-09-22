using System;

namespace Sachssoft.Engine.Resources.Localization;

/// <summary>
/// Provides helper methods for accessing and binding localized strings.
/// </summary>
public static class Localized
{
    /// <summary>
    /// Creates a localization binding for the specified application.
    /// </summary>
    public static LocalizationBinding Bind(
        GameApplicationBase application,
        string key,
        string? defaultValue,
        Action<string?> setter)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(setter);

        return new LocalizationBinding(
            application,
            key,
            defaultValue,
            setter);
    }

    /// <summary>
    /// Gets a localized string for the specified application.
    /// </summary>
    public static string? GetValue(
        IGameApplication application,
        string key,
        string? defaultValue = null)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return application.Localization.Entries.GetValue(
            key,
            defaultValue);
    }
}
