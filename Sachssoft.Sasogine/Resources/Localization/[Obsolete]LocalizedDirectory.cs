//using Sachssoft.Sasogine.Resources;
//using Sachssoft.Sasogine.Resources.Loaders;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Xml.Linq;

//namespace Sachssoft.Sasogine.Resources.Localization
//{
//    public class LocalizedDictionary
//    {
//        private bool _isImmutable;

//        private static readonly Dictionary<string, RegisteredTypeEntry> _registeredTypes
//            = new(StringComparer.OrdinalIgnoreCase);

//        private readonly Dictionary<string, EntryWrapper> _entries
//            = new(StringComparer.OrdinalIgnoreCase);

//        private class RegisteredTypeEntry
//        {
//            public required string TypeName { get; init; }
//            public required Type Type { get; init; }
//            public required Func<ILocalizedEntry> Factory { get; init; }
//        }

//        private class EntryWrapper
//        {
//            public readonly string TypeName;
//            public readonly LocalizedEntryData Data;
//            public ILocalizedEntry? Instance;

//            public EntryWrapper(string typeName, LocalizedEntryData data)
//            {
//                TypeName = typeName;
//                Data = data;
//            }
//        }

//        static LocalizedDictionary()
//        {
//            Register<LocalizedString>("String");
//            Register<LocalizedResource>("Resource");
//        }

//        public static readonly LocalizedDictionary Empty = new LocalizedDictionary()
//        {
//            _isImmutable = true
//        };

//        public static void Register<T>(string typeName)
//            where T : class, ILocalizedEntry, new()
//        {
//            if (string.IsNullOrWhiteSpace(typeName))
//                throw new ArgumentException("Type name cannot be null or empty.", nameof(typeName));

//            if (_registeredTypes.ContainsKey(typeName))
//                throw new ArgumentException($"Type '{typeName}' is already registered.");

//            _registeredTypes[typeName] = new RegisteredTypeEntry()
//            {
//                Type = typeof(T),
//                TypeName = typeName,
//                Factory = () => new T()
//            };
//        }

//        public static LocalizedDictionary Load(GameApplication application, LoaderBase loader)
//        {
//            if (loader == null) throw new ArgumentNullException(nameof(loader));

//            XDocument doc;
//            using (var stream = loader.GetStream())
//                doc = XDocument.Load(stream);

//            if (doc.Root == null || doc.Root.Name.LocalName != "Localization")
//                throw new Exception("Invalid or empty localization XML.");

//            var dictionary = new LocalizedDictionary();

//            foreach (var element in doc.Root.Elements())
//            {
//                var id = element.Attribute("Id")?.Value;
//                if (string.IsNullOrEmpty(id))
//                {
//                    //Console.WriteLine($"Warning: Missing 'Id' in element {element.Name}");
//                    continue;
//                }

//                string? content = null;
//                Dictionary<LocalizationPluralCase, string?> pluralCases = new();
//                Dictionary<string, string?> attributes = element.Attributes()
//                    .Where(a => a.Name != "Id")
//                    .ToDictionary(a => a.Name.LocalName, a => (string?)a.Value);

//                if (!element.HasElements)
//                {
//                    // Variante 1: Kein Child-Element → gesamten Text holen
//                    content = element.Attribute("Value")?.Value ?? element.Value.Trim();
//                }
//                else
//                {
//                    // Variante 2: Child <Case>-Elemente → Varianten
//                    foreach (var caseElement in element.Elements("Case"))
//                    {
//                        var ruleStr = caseElement.Attribute("Rule")?.Value ?? "Default";
//                        if (Enum.TryParse<LocalizationPluralCase>(ruleStr, true, out var rule))
//                            pluralCases[rule] = caseElement.Value.Trim();
//                        else
//                            pluralCases[LocalizationPluralCase.Default] = caseElement.Value.Trim();
//                    }
//                }

//                var typeName = element.Name.LocalName;
//                if (_registeredTypes.TryGetValue(typeName, out var entry))
//                {
//                    dictionary.AddEntry(id, typeName, new LocalizedEntryData()
//                    {
//                        Id = id,
//                        Content = content,
//                        PluralCases = pluralCases,
//                        Attributes = attributes
//                    });
//                }
//            }

//            dictionary.Close();
//            return dictionary;
//        }

//        /// <summary>
//        /// Fügt einen Eintrag hinzu. Es wird nichts sofort geladen.
//        /// </summary>
//        protected void AddEntry(string key, string typeName, LocalizedEntryData data)
//        {
//            if (_isImmutable)
//                throw new InvalidOperationException("Cannot modify a closed LocalizedDictionary.");

//            if (!_registeredTypes.ContainsKey(typeName))
//                throw new InvalidOperationException($"Type '{typeName}' is not registered.");

//            _entries[key] = new EntryWrapper(typeName, data);
//        }

//        private ILocalizedEntry EnsureEntryLoaded(string key)
//        {
//            if (!_entries.TryGetValue(key, out var wrapper))
//                throw new KeyNotFoundException($"Entry '{key}' not found.");

//            if (wrapper.Instance == null)
//            {
//                var typeEntry = _registeredTypes[wrapper.TypeName];
//                var instance = typeEntry.Factory();
//                instance.Load(System.Globalization.CultureInfo.InvariantCulture, null!, wrapper.Data);
//                wrapper.Instance = instance;
//            }

//            return wrapper.Instance;
//        }

//        public bool ContainsKey(string key) => _entries.ContainsKey(key);

//        public bool IsValueLoaded(string key)
//            => _entries.TryGetValue(key, out var wrapper) && wrapper.Instance != null && wrapper.Instance.IsLoaded;

//        public bool TryGetValue<T>(string key, out T? result, T? defaultValue = default)
//           where T : class
//        {
//            return TryGetValue<T>(key, 0, out result, defaultValue);
//        }

//        public bool TryGetValue<T>(string key, int quantity, out T? result, T? defaultValue = default)
//            where T : class
//        {
//            if (_entries.ContainsKey(key))
//            {
//                var entry = EnsureEntryLoaded(key);
//                if (entry.GetValue(quantity) is T t)
//                {
//                    result = t;
//                    return true;
//                }
//            }

//            result = defaultValue;
//            return false;
//        }

//        public T? GetValue<T>(string key, T? defaultValue = default)
//            where T : class
//        {
//            return GetValue<T>(key, 0, defaultValue);
//        }

//        public T? GetValue<T>(string key, int quantity, T? defaultValue = default)
//            where T : class
//        {
//            if (TryGetValue<T>(key, quantity, out var result))
//                return result;
//            return defaultValue;
//        }

//        public void Close() => _isImmutable = true;
//    }
//}
