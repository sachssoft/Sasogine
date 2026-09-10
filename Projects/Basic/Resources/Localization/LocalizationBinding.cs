using System;

namespace Sachssoft.Sasogine.Resources.Localization;

/// <summary>
/// Binds a localized value to a property or setter and automatically updates
/// the value when the current culture changes.
/// </summary>
/// <typeparam name="T">
/// The type of the localized value.
/// </typeparam>
public sealed class LocalizationBinding<T> : IDisposable
    where T : class
{
    private readonly GameApplicationBase _application;
    private readonly string _key;
    private readonly T? _defaultValue;
    private readonly Action<T?> _setter;

    private bool _disposed;

    internal LocalizationBinding(
        GameApplicationBase application,
        string key,
        T? defaultValue,
        Action<T?> setter)
    {
        _application =
            application ??
            throw new ArgumentNullException(nameof(application));

        _key =
            key ??
            throw new ArgumentNullException(nameof(key));

        _defaultValue = defaultValue;

        _setter =
            setter ??
            throw new ArgumentNullException(nameof(setter));

        _application.Localization.CurrentCultureChanged +=
            OnCurrentCultureChanged;

        UpdateValue();
    }

    private void OnCurrentCultureChanged(
        object? sender,
        EventArgs e)
    {
        UpdateValue();
    }

    private void UpdateValue()
    {
        if (_disposed)
            return;

        var entries =
            _application.Localization.Entries;

        if (!entries.TryGetValue<T>(_key, out var value))
            value = _defaultValue;

        _setter(value);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _application.Localization.CurrentCultureChanged -=
            OnCurrentCultureChanged;

        _disposed = true;
    }
}