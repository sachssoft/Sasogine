using Sachssoft.Sasogine.Localization;
using System;

namespace Sachssoft.Sasogine.Surface.Basic
{
    /// <summary>
    /// Binds a localized value to a property or setter, automatically updating when the language changes.
    /// </summary>
    /// <typeparam name="T">The type of the localized value wrapper (e.g., <see cref="LocalizedString"/>).</typeparam>
    public sealed class LocalizationBinding<T> : IDisposable
    {
        private readonly string _key;
        private readonly T? _defaultValue;
        private readonly Action<T?> _setter;

        /// <summary>
        /// Creates a new localization binding for a given key.
        /// </summary>
        /// <param name="key">The localization key.</param>
        /// <param name="defaultValue">The value to use if the key is not found.</param>
        /// <param name="setter">Action that sets the value in the UI or property.</param>
        internal LocalizationBinding(string key, T? defaultValue, Action<T?> setter)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _defaultValue = defaultValue;
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));

            // Initialwert setzen
            UpdateValue();

            // Listener auf LanguageChanged
            GameApplication.Current.Localization.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            UpdateValue();
        }

        private void UpdateValue()
        {
            // Versuche den Wert aus der aktuellen Dictionary zu holen
            if (GameApplication.Current.Localization.Entries.ContainsKey(_key))
            {
                _setter.Invoke(GameApplication.Current.Localization.Entries.GetValue(_key, _defaultValue));
            }
            else
            {
                // Key nicht gefunden -> default setzen
                _setter.Invoke(_defaultValue);
            }
        }

        /// <summary>
        /// Unsubscribes from language changes and releases resources.
        /// </summary>
        public void Dispose()
        {
            GameApplication.Current.Localization.LanguageChanged -= OnLanguageChanged;
        }
    }
}
