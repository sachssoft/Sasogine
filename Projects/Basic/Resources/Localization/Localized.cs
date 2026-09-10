using System;

namespace Sachssoft.Sasogine.Resources.Localization;

/// <summary>
/// Provides helper methods for accessing and binding localized values.
/// </summary>
public static class Localized
{
    /// <summary>
    /// Creates a localization binding for the specified application.
    /// </summary>
    public static LocalizationBinding<T> Bind<T>(
        GameApplicationBase application,
        string key,
        T? defaultValue,
        Action<T?> setter)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(application);

        return new LocalizationBinding<T>(
            application,
            key,
            defaultValue,
            setter);
    }

    /// <summary>
    /// Gets a localized value for the specified application.
    /// </summary>
    public static T? GetValue<T>(
        IGameApplication application,
        string key,
        T? defaultValue)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(key);

        var entries =
            application.Localization.Entries;

        if (entries.TryGetValue<T>(key: key, out var value))
            return value;

        return defaultValue;
    }
}