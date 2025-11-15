using Sachssoft.Sasogine.Resources.Loaders;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sachssoft.Sasogine.Localization
{
    public sealed class LocalizationManager
    {
        private readonly Dictionary<string, LocalizedDictionary> _dictionaries = new();
        private CultureInfo _currentCulture = CultureInfo.InvariantCulture;
        private readonly GameApplication _application;

        public event Action? LanguageChanged;

        public LocalizationManager(GameApplication application)
        {
            _application = application;
        }

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (!_currentCulture.Equals(value))
                {
                    _currentCulture = value;
                    CultureInfo.CurrentUICulture = value;
                    CultureInfo.CurrentCulture = value;

                    LanguageChanged?.Invoke();
                }
            }
        }

        public LocalizedDictionary Entries
            => _dictionaries.TryGetValue(_currentCulture.Name, out var dict) ? dict : LocalizedDictionary.Empty;

        public void Add(CultureInfo culture, LoaderBase loader)
        {
            _dictionaries[culture.Name] = LocalizedDictionary.Load(_application, loader);
        }

        public void AddEmbedded(CultureInfo culture, string filePath)
        {
            _dictionaries[culture.Name] = LocalizedDictionary.Load(_application, new EmbeddedResourceLoader(filePath));
        }

        public void AddLocal(CultureInfo culture, string filePath)
        {
            _dictionaries[culture.Name] = LocalizedDictionary.Load(_application, new LocalFileLoader(filePath));
        }

        public void Close()
        {
            foreach (var dict in _dictionaries.Values)
                dict.Close();
        }
    }
}
