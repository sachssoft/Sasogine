using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources.Localization
{
    public class LocalizedDictionary
    {
        private bool _isImmutable;

        private readonly Dictionary<string, EntryWrapper> _entries = new(StringComparer.OrdinalIgnoreCase);
        private readonly LocalizedTypeRegistry _typeRegistry;

        private class EntryWrapper
        {
            public readonly string TypeName;
            public readonly LocalizedEntryData Data;
            public ILocalizedEntry? Instance;

            public EntryWrapper(string typeName, LocalizedEntryData data)
            {
                TypeName = typeName;
                Data = data;
            }
        }

        public LocalizedDictionary() : this(new LocalizedTypeRegistry()) { }

        public LocalizedDictionary(LocalizedTypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        /// <summary>
        /// Fügt einen Eintrag hinzu. Es wird nichts sofort geladen.
        /// </summary>
        protected void AddEntry(string key, string typeName, LocalizedEntryData data)
        {
            if (_isImmutable)
                throw new InvalidOperationException("Cannot modify a closed LocalizedDictionary.");

            if (!_typeRegistry.IsRegistered(typeName))
                throw new InvalidOperationException($"Type '{typeName}' is not registered.");

            _entries[key] = new EntryWrapper(typeName, data);
        }

        private ILocalizedEntry EnsureEntryLoaded(string key)
        {
            if (!_entries.TryGetValue(key, out var wrapper))
                throw new KeyNotFoundException($"Entry '{key}' not found.");

            if (wrapper.Instance == null)
            {
                var instance = _typeRegistry.Create(wrapper.TypeName);
                instance.Load(System.Globalization.CultureInfo.InvariantCulture, null!, wrapper.Data);
                wrapper.Instance = instance;
            }

            return wrapper.Instance;
        }

        public bool ContainsKey(string key) => _entries.ContainsKey(key);

        public bool IsValueLoaded(string key)
            => _entries.TryGetValue(key, out var wrapper) && wrapper.Instance != null && wrapper.Instance.IsLoaded;

        public bool TryGetValue<T>(string key, out T? result, T? defaultValue = default)
           where T : class
        {
            return TryGetValue<T>(key, 0, out result, defaultValue);
        }

        public bool TryGetValue<T>(string key, int quantity, out T? result, T? defaultValue = default)
            where T : class
        {
            if (_entries.ContainsKey(key))
            {
                var entry = EnsureEntryLoaded(key);
                if (entry.GetValue(quantity) is T t)
                {
                    result = t;
                    return true;
                }
            }

            result = defaultValue;
            return false;
        }

        public T? GetValue<T>(string key, T? defaultValue = default)
            where T : class
        {
            return GetValue<T>(key, 0, defaultValue);
        }

        public T? GetValue<T>(string key, int quantity, T? defaultValue = default)
            where T : class
        {
            if (TryGetValue<T>(key, quantity, out var result))
                return result;
            return defaultValue;
        }

        public void Close() => _isImmutable = true;
    }
}
