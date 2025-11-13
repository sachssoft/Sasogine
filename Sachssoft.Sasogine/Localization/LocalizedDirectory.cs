using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

namespace Sachssoft.Sasogine.Localization
{
    /// <summary>
    /// Represents a localization dictionary that maps string keys to localized entries.
    /// Supports lazy loading of entry wrappers using a factory system.
    /// Once closed, the dictionary becomes immutable and cannot be modified.
    /// </summary>
    public class LocalizedDictionary
    {
        /// <summary>
        /// Indicates whether the dictionary is immutable/closed.
        /// </summary>
        private bool _isImmutable;

        /// <summary>
        /// Source type for entries (Embedded or Local file).
        /// </summary>
        private LocalizationSource _entrySource = LocalizationSource.Embedded;

        /// <summary>
        /// Loader options used when creating entry wrappers.
        /// </summary>
        private LoaderOptions _options;

        /// <summary>
        /// Registered wrapper types for each type name.
        /// </summary>
        private static readonly Dictionary<string, RegisteredTypeEntry> _registeredTypes
            = new Dictionary<string, RegisteredTypeEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Dictionary of entries, keyed by string.
        /// </summary>
        private readonly Dictionary<string, Entry> _entries
            = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Represents a registered wrapper type with a factory.
        /// </summary>
        private class RegisteredTypeEntry
        {
            /// <summary>
            /// The type name used in the XML file.
            /// </summary>
            public required string TypeName { get; init; }

            /// <summary>
            /// CLR type of the wrapper.
            /// </summary>
            public required Type Type { get; init; }

            /// <summary>
            /// Factory used to create wrapper instances.
            /// </summary>
            public required Action<LocalizationEntryFactoryContext> Factory { get; init; }
        }

        /// <summary>
        /// Represents an individual entry in the dictionary.
        /// </summary>
        private class Entry
        {
            /// <summary>
            /// The type name of the wrapper.
            /// </summary>
            public required string TypeName { get; init; }

            /// <summary>
            /// Attributes from the XML element.
            /// </summary>
            public required Dictionary<string, string?> Attributes { get; init; }

            /// <summary>
            /// Cached wrapper instance for lazy loading.
            /// </summary>
            public ILocalizedEntryWrapper? WrapperCache { get; set; }
        }

        /// <summary>
        /// Static constructor to register default wrapper types.
        /// </summary>
        static LocalizedDictionary()
        {
            Register<LocalizedString>("String", ctx => ctx.Instance = Activator.CreateInstance<LocalizedString>);
            Register<LocalizedTexture>("Texture", ctx =>
            {
                if (!ctx.Attributes.TryGetValue("FilePath", out var filename) || string.IsNullOrEmpty(filename))
                {
                    ctx.Cancel = true;
                    return;
                }

                ctx.FilePath = filename;
                ctx.Instance = () => new LocalizedTexture(ctx.LoaderOptions.GraphicsDevice);
            });
            Register<LocalizedSound>("Sound", ctx =>
            {
                if (!ctx.Attributes.TryGetValue("FilePath", out var filename) || string.IsNullOrEmpty(filename))
                {
                    ctx.Cancel = true;
                    return;
                }

                ctx.FilePath = filename;
                ctx.Instance = () => new LocalizedSound();
            });
        }

        /// <summary>
        /// Initializes a new empty instance of <see cref="LocalizedDictionary"/> with default loader options.
        /// </summary>
        public LocalizedDictionary() : this(new LoaderOptions()) { }

        /// <summary>
        /// Initializes a new instance of <see cref="LocalizedDictionary"/> with optional loader options and read-only flag.
        /// </summary>
        /// <param name="options">Loader options used for wrapper creation.</param>
        /// <param name="isReadOnly">If true, the dictionary will be immutable.</param>
        public LocalizedDictionary(LoaderOptions? options = null, bool isReadOnly = false)
        {
            _options = options ?? new LoaderOptions();
            _isImmutable = isReadOnly;
        }

        /// <summary>
        /// Registers a wrapper type for a specific localization type name.
        /// </summary>
        /// <typeparam name="T">Type implementing <see cref="ILocalizedEntryWrapper"/>.</typeparam>
        /// <param name="typeName">Type name as defined in the XML file.</param>
        /// <param name="factory">Factory delegate to create wrapper instances.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="typeName"/> is null or empty, or type is already registered.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> is null.</exception>
        public static void Register<T>(string typeName, Action<LocalizationEntryFactoryContext> factory)
            where T : class, ILocalizedEntryWrapper
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Type name cannot be null or empty.", nameof(typeName));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (_registeredTypes.ContainsKey(typeName))
                throw new ArgumentException($"Type '{typeName}' is already registered.");

            _registeredTypes[typeName] = new RegisteredTypeEntry()
            {
                TypeName = typeName,
                Factory = factory,
                Type = typeof(T)
            };
        }

        /// <summary>
        /// Loads a <see cref="LocalizedDictionary"/> from an XML loader.
        /// </summary>
        /// <param name="loader">Loader providing access to the XML content.</param>
        /// <param name="options">Loader options for wrapper creation.</param>
        /// <param name="entrySource">Source type (Embedded or Local).</param>
        /// <returns>A populated <see cref="LocalizedDictionary"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="loader"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="Exception">Thrown if XML file is empty or invalid.</exception>
        public static LocalizedDictionary Load(LoaderBase loader, LoaderOptions options, LocalizationSource entrySource)
        {
            if (loader == null) throw new ArgumentNullException(nameof(loader));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var doc = XDocument.Load(loader.GetStream());
            if (doc.Root == null)
                throw new Exception("The XML file is empty or invalid.");

            var dictionary = new LocalizedDictionary(options) { _entrySource = entrySource };

            foreach (var element in doc.Root.Elements())
            {
                var id = element.Attribute("Id")?.Value;
                if (string.IsNullOrEmpty(id))
                {
                    Console.WriteLine($"Warning: Missing 'Id' attribute in element {element.Name}");
                    continue;
                }

                var attributes = element.Attributes()
                    .Where(a => a.Name != "Id")
                    .ToDictionary(a => a.Name.LocalName, a => (string?)a.Value);

                dictionary.AddEntry(id, element.Name.LocalName, attributes);
            }

            dictionary.Close();
            return dictionary;
        }

        /// <summary>
        /// Adds a new entry to the dictionary without creating the wrapper immediately.
        /// </summary>
        /// <param name="key">The unique key of the entry.</param>
        /// <param name="typeName">The type name of the wrapper.</param>
        /// <param name="attributes">Dictionary of attributes from the XML element.</param>
        /// <exception cref="InvalidOperationException">Thrown if the dictionary is closed.</exception>
        public void AddEntry(string key, string typeName, Dictionary<string, string?> attributes)
        {
            if (_isImmutable)
                throw new InvalidOperationException("Cannot modify a closed LocalizedDictionary.");

            _entries[key] = new Entry()
            {
                Attributes = attributes,
                TypeName = typeName,
                WrapperCache = null
            };
        }

        /// <summary>
        /// Checks if a wrapper has already been created for the given key.
        /// </summary>
        /// <param name="key">Key to check.</param>
        /// <returns>True if the wrapper is loaded; otherwise false.</returns>
        public bool IsValueLoaded(string key)
        {
            return _entries.TryGetValue(key, out var entry) && entry.WrapperCache != null;
        }

        /// <summary>
        /// Checks if the entry for the given key is of the specified wrapper type.
        /// </summary>
        /// <typeparam name="TWrapper">Wrapper type to check.</typeparam>
        /// <param name="key">Key to check.</param>
        /// <returns>True if entry is of type <typeparamref name="TWrapper"/>; otherwise false.</returns>
        public bool IsValueOf<TWrapper>(string key) where TWrapper : class, ILocalizedEntryWrapper
        {
            if (!_entries.TryGetValue(key, out var entry0))
                return false;

            if (!_registeredTypes.TryGetValue(entry0.TypeName, out var entry1))
                return false;

            return entry1.Type == typeof(TWrapper);
        }

        /// <summary>
        /// Checks whether the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">Key to check.</param>
        /// <returns>True if the key exists; otherwise false.</returns>
        public bool ContainsKey(string key) => _entries.ContainsKey(key);

        /// <summary>
        /// Tries to get the value for the specified key and type, creating the wrapper lazily if necessary.
        /// </summary>
        /// <typeparam name="T">Expected wrapper value type.</typeparam>
        /// <param name="key">The entry key.</param>
        /// <param name="result">Outputs the value if successful; otherwise returns <paramref name="defaultValue"/>.</param>
        /// <param name="defaultValue">Value returned if the key is not found or wrapper creation is cancelled.</param>
        /// <returns>True if the key exists and value could be cast to <typeparamref name="T"/>; otherwise false.</returns>
        public bool TryGetValue<T>(string key, out T? result, T? defaultValue = default)
        {
            result = defaultValue;

            if (!_entries.TryGetValue(key, out var entry))
                return false;

            if (entry.WrapperCache == null)
            {
                if (!_registeredTypes.TryGetValue(entry.TypeName, out var regEntry))
                    return false;

                var context = new LocalizationEntryFactoryContext(entry.Attributes, _options);
                regEntry.Factory.Invoke(context);

                if (context.Cancel)
                    return true;

                if (context.Instance == null)
                    throw new InvalidOperationException($"Factory for '{entry.TypeName}' did not set Instance.");

                var wrapper = context.Instance.Invoke();
                LoaderBase loader = _entrySource switch
                {
                    LocalizationSource.Embedded => new EmbeddedResourceLoader(context.FilePath),
                    _ => new LocalFileLoader(context.FilePath)
                };

                wrapper.Load(entry.Attributes, loader);
                entry.WrapperCache = wrapper;
            }

            if (entry.WrapperCache.Value is T typedValue)
            {
                result = typedValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the value for the specified key and type, creating the wrapper lazily if necessary.
        /// Throws an exception if the key is missing or the type does not match.
        /// </summary>
        /// <typeparam name="T">Expected wrapper value type.</typeparam>
        /// <param name="key">The entry key.</param>
        /// <param name="defaultValue">Value returned if wrapper creation is cancelled.</param>
        /// <returns>The value of type <typeparamref name="T"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the wrapper type is not registered or factory did not provide an instance.</exception>
        /// <exception cref="InvalidCastException">Thrown if the loaded wrapper cannot be cast to <typeparamref name="T"/>.</exception>
        public T? GetValue<T>(string key, T? defaultValue = default)
        {
            if (!_entries.TryGetValue(key, out var entry))
                throw new KeyNotFoundException($"Key '{key}' not found in the localization dictionary.");

            if (entry.WrapperCache == null)
            {
                if (!_registeredTypes.TryGetValue(entry.TypeName, out var regEntry))
                    throw new InvalidOperationException($"Type '{entry.TypeName}' for key '{key}' is not registered.");

                var context = new LocalizationEntryFactoryContext(entry.Attributes, _options);
                regEntry.Factory.Invoke(context);

                if (context.Cancel)
                    return defaultValue;

                if (context.Instance == null)
                    throw new InvalidOperationException($"Factory for key '{key}' did not provide an instance.");

                var wrapper = context.Instance.Invoke();
                LoaderBase? loader = null;

                if (!string.IsNullOrEmpty(context.FilePath))
                {
                    loader = _entrySource switch
                    {
                        LocalizationSource.Embedded => new EmbeddedResourceLoader(context.FilePath),
                        _ => new LocalFileLoader(context.FilePath)
                    };
                }

                wrapper.Load(entry.Attributes, loader);
                entry.WrapperCache = wrapper;
            }

            if (entry.WrapperCache.Value is T typedValue)
                return typedValue;

            throw new InvalidCastException($"Value for key '{key}' is of type {entry.WrapperCache.Value?.GetType().Name}, not {typeof(T).Name}.");
        }

        /// <summary>
        /// Closes the dictionary, making it immutable.
        /// Safe to call multiple times.
        /// </summary>
        public void Close()
        {
            _isImmutable = true;
        }
    }
}
