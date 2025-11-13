using Sachssoft.Sasogine.Localization;
using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Sachssoft.Sasogine.Localization
{
    /// <summary>
    /// Manages localization dictionaries and the current culture for an application.
    /// Supports lazy-loading of dictionaries and automatic switching when <see cref="CurrentCulture"/> changes.
    /// </summary>
    /// <remarks>
    /// Dictionaries added via <see cref="Add(CultureInfo, LoaderBase, LocalizationSource)"/>, 
    /// <see cref="AddLocal(CultureInfo, string)"/> or <see cref="AddEmbeddedResource(CultureInfo, string)"/> 
    /// are loaded lazily when the corresponding culture is first accessed through <see cref="Entries"/> 
    /// or when <see cref="CurrentCulture"/> is set to that culture.
    /// After <see cref="Close"/> is called, the manager becomes immutable and cannot be modified.
    /// </remarks>
    public sealed class LocalizationManager
    {
        private readonly Dictionary<string, LazyDictionary> _dictionaries = new();
        private CultureInfo _currentCulture = CultureInfo.InvariantCulture;
        private bool _isImmutable = false;
        private readonly LocalizedDictionary _emptyDictionary = new(isReadOnly: true);

        /// <summary>
        /// Represents a dictionary that is loaded lazily on first access.
        /// </summary>
        private class LazyDictionary
        {
            /// <summary>
            /// Loader used to obtain the localization XML file or resource.
            /// </summary>
            public LoaderBase Loader { get; init; }

            /// <summary>
            /// Specifies the source type of the localization data.
            /// </summary>
            public LocalizationSource EntrySource { get; init; }

            /// <summary>
            /// Cached instance of the loaded dictionary. Null until loaded.
            /// </summary>
            public LocalizedDictionary? Dictionary { get; set; }

            /// <summary>
            /// Initializes a new instance of <see cref="LazyDictionary"/>.
            /// </summary>
            /// <param name="loader">Loader used to read the XML or resource.</param>
            /// <param name="source">Source type (Embedded or Local).</param>
            public LazyDictionary(LoaderBase loader, LocalizationSource source)
            {
                Loader = loader;
                EntrySource = source;
            }

            /// <summary>
            /// Returns the loaded dictionary, loading it if necessary.
            /// </summary>
            /// <returns>The loaded <see cref="LocalizedDictionary"/>.</returns>
            public LocalizedDictionary GetOrLoad()
            {
                if (Dictionary == null)
                {
                    Dictionary = LocalizedDictionary.Load(Loader, new LoaderOptions(), EntrySource);
                }
                return Dictionary;
            }
        }

        /// <summary>
        /// Occurs when the current language/culture changes.
        /// Subscribers can update UI or reload localized content.
        /// </summary>
        public event Action? LanguageChanged;

        /// <summary>
        /// Gets or sets the current culture used for localization.
        /// Setting this property switches the active dictionary to the corresponding culture if available
        /// and raises <see cref="LanguageChanged"/>.
        /// </summary>
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

                    // Lazy load the dictionary for the new culture, if available
                    _dictionaries.TryGetValue(_currentCulture.Name, out var lazy);
                    lazy?.GetOrLoad();

                    LanguageChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Gets the currently active localization dictionary.
        /// If no dictionary exists for the current culture, returns an empty read-only dictionary.
        /// </summary>
        public LocalizedDictionary Entries
        {
            get
            {
                if (_dictionaries.TryGetValue(_currentCulture.Name, out var lazy))
                {
                    var dict = lazy.GetOrLoad();
                    if (_isImmutable) dict.Close();
                    return dict;
                }
                return _emptyDictionary;
            }
        }

        /// <summary>
        /// Adds a localization dictionary for the specified culture using a <see cref="LoaderBase"/>.
        /// The dictionary is not loaded immediately; it is loaded on first access.
        /// </summary>
        /// <param name="culture">The culture the dictionary applies to.</param>
        /// <param name="dictionaryLoader">A loader providing access to the localization XML file or embedded resource.</param>
        /// <param name="entrySource">Specifies where the localization data is loaded from.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="culture"/> or <paramref name="dictionaryLoader"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the manager is immutable.</exception>
        public void Add(CultureInfo culture, LoaderBase dictionaryLoader, LocalizationSource entrySource = LocalizationSource.Local)
        {
            ThrowIfImmutable();

            if (culture == null) throw new ArgumentNullException(nameof(culture));
            if (dictionaryLoader == null) throw new ArgumentNullException(nameof(dictionaryLoader));

            _dictionaries[culture.Name] = new LazyDictionary(dictionaryLoader, entrySource);
        }

        /// <summary>
        /// Adds a localization dictionary for the specified culture from a local file path.
        /// Internally wraps the file path in a <see cref="LocalFileLoader"/>.
        /// </summary>
        /// <param name="culture">The culture the dictionary applies to.</param>
        /// <param name="dictionaryFilePath">The path to the localization XML file.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="culture"/> is null or <paramref name="dictionaryFilePath"/> is null or empty.</exception>
        public void AddLocal(CultureInfo culture, string dictionaryFilePath)
        {
            ThrowIfImmutable();

            if (string.IsNullOrWhiteSpace(dictionaryFilePath))
                throw new ArgumentNullException(nameof(dictionaryFilePath));

            Add(culture, new LocalFileLoader(dictionaryFilePath), LocalizationSource.Local);
        }

        /// <summary>
        /// Adds a localization dictionary for the specified culture from an embedded resource.
        /// Internally wraps the resource path in an <see cref="EmbeddedResourceLoader"/>.
        /// </summary>
        /// <param name="culture">The culture the dictionary applies to.</param>
        /// <param name="dictionaryResourceFilePath">The embedded resource path to the localization XML file.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="culture"/> is null or <paramref name="dictionaryResourceFilePath"/> is null or empty.</exception>
        public void AddEmbeddedResource(CultureInfo culture, string dictionaryResourceFilePath, Assembly? assembly = null)
        {
            ThrowIfImmutable();

            if (string.IsNullOrWhiteSpace(dictionaryResourceFilePath))
                throw new ArgumentNullException(nameof(dictionaryResourceFilePath));

            Add(culture, new EmbeddedResourceLoader(
                    dictionaryResourceFilePath, 
                    assembly ?? Assembly.GetExecutingAssembly()
                ), LocalizationSource.Embedded);
        }


        /// <summary>
        /// Marks the manager as immutable. After calling this method, no new dictionaries can be added,
        /// and all already loaded dictionaries are closed to prevent further modifications.
        /// </summary>
        public void Close()
        {
            _isImmutable = true;

            foreach (var lazy in _dictionaries.Values)
            {
                lazy.Dictionary?.Close();
            }
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the manager is immutable.
        /// </summary>
        private void ThrowIfImmutable()
        {
            if (_isImmutable)
                throw new InvalidOperationException("Cannot modify a closed LocalizationManager.");
        }
    }
}
