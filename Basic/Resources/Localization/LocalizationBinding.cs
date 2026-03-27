using System;

namespace Sachssoft.Sasogine.Resources.Localization
{
    /// <summary>
    /// Binds a localized value to a property or setter, automatically updating when the language changes.
    /// </summary>
    /// <typeparam name="T">Type of localized wrapper, e.g., LocalizedString.</typeparam>
    public sealed class LocalizationBinding<T> : IDisposable
        where T : class
    {
        private readonly GameApplication _application;
        private readonly string _key;
        private readonly T? _defaultValue;
        private readonly Action<T?> _setter;
        private bool _disposed;

        internal LocalizationBinding(GameApplication application, string key, T? defaultValue, Action<T?> setter)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _defaultValue = defaultValue;
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));

            // Listener auf LanguageChanged
            _application = application;
            _application.Localization.CurrentCultureChanged += OnCurrentCultureChanged;

            // Initialwert setzen
            UpdateValue();
        }

        private void OnCurrentCultureChanged(object? sender, EventArgs e) => UpdateValue();

        private void UpdateValue()
        {
            if (_disposed) return;

            var dict = GameApplication.Current.Localization.Entries;

            if (!dict.TryGetValue<T>(_key, out var value))
                value = _defaultValue;

            _setter.Invoke(value);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _application.Localization.CurrentCultureChanged -= OnCurrentCultureChanged;
            _disposed = true;
        }
    }
}
