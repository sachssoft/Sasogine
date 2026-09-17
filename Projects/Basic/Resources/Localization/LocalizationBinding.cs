using System;

namespace Sachssoft.Sasogine.Resources.Localization;

/// <summary>
/// Binds a localized string to a setter and updates it when the language changes.
/// </summary>
public sealed class LocalizationBinding : IDisposable
{
    private readonly GameApplicationBase _application;
    private readonly string _key;
    private readonly string? _fallback;
    private readonly Action<string?> _setter;
    private bool _disposed;

    internal LocalizationBinding(
        GameApplicationBase application,
        string key,
        string? fallback,
        Action<string?> setter)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(setter);

        _application = application;
        _key = key;
        _fallback = fallback;
        _setter = setter;

        _application.Localization.LanguageChanged += OnLanguageChanged;
        UpdateValue();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _application.Localization.LanguageChanged -= OnLanguageChanged;
        _disposed = true;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        UpdateValue();
    }

    private void UpdateValue()
    {
        if (_disposed)
            return;

        _setter(_application.Localization.Entries.GetValue(_key, _fallback));
    }
}