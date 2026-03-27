using Sachssoft.Sasogine.Resources.Loaders;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sachssoft.Sasogine.Resources.Localization
{
    public sealed class LocalizationManager
    {
        private readonly Dictionary<string, LocalizedDictionary> _dictionaries = new();
        private readonly GameApplication _application;
        private readonly LocalizedDictionary _emptyDictionary = new LocalizedDictionary();

        private CultureInfo _currentCulture = CultureInfo.InvariantCulture;

        public event EventHandler? CurrentCultureChanged;

        public LocalizationManager(GameApplication application)
        {
            _application = application;
        }

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (_currentCulture.Equals(value))
                    return;

                _currentCulture = value;

                CultureInfo.CurrentUICulture = value;
                CultureInfo.DefaultThreadCurrentUICulture = value;

                CurrentCultureChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public LocalizedDictionary Entries
            => _dictionaries.TryGetValue(_currentCulture.Name, out var dict) ? dict : _emptyDictionary;

        public void Add(CultureInfo culture, LocalizedDictionary dictionary)
        {
            _dictionaries[culture.Name] = dictionary;
        }

        public void Close()
        {
            foreach (var dict in _dictionaries.Values)
                dict.Close();
        }
    }
}
