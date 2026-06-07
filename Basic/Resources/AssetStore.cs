using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Resources.Loaders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources;

public partial class AssetStore
{
    private static readonly Dictionary<Type, Delegate> _registeredLoaders = new Dictionary<Type, Delegate>();
    private static readonly Dictionary<Type, object> _registeredFactories = new Dictionary<Type, object>();

    // Culture-specific assets: CultureInfo → Key → ResourceEntry
    private readonly Dictionary<CultureInfo, Dictionary<string, object>> _items =
        new Dictionary<CultureInfo, Dictionary<string, object>>();

    private readonly GameApplicationBase _gameApplication;

    public AssetStore(GameApplicationBase application)
    {
        _gameApplication = application ?? throw new ArgumentNullException(nameof(application));
    }

    public GameApplicationBase GameApplication => _gameApplication;

    public GraphicsDevice GraphicsDevice => _gameApplication.GraphicsDevice;

    public string RootDirectory
    {
        get => _gameApplication.Content.RootDirectory;
        set => _gameApplication.Content.RootDirectory = value;
    }

    public static void RegisterLoader<TLoader>(Func<AssetStore, string, TLoader> factory)
        where TLoader : ResourceSourceBase
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        _registeredLoaders[typeof(TLoader)] = factory;
    }

    private TLoader CreateLoader<TLoader>(string path) where TLoader : ResourceSourceBase
    {
        if (!_registeredLoaders.TryGetValue(typeof(TLoader), out var factory))
            throw new InvalidOperationException($"No loader factory registered for type {typeof(TLoader).Name}.");

        return (TLoader)factory.DynamicInvoke(this, path)!;
    }

    public static void RegisterType<TData>(
        Func<AssetStore, ResourceSourceBase, TData>? syncFactory = null,
        Func<AssetStore, ResourceSourceBase, Task<TData>>? asyncFactory = null)
    {
        if (syncFactory == null && asyncFactory == null)
            throw new ArgumentException("At least one of syncFactory or asyncFactory must be provided.");

        if (_registeredFactories.ContainsKey(typeof(TData)))
            throw new InvalidOperationException($"Factory for type {typeof(TData).Name} is already registered.");

        _registeredFactories[typeof(TData)] = new FactoryWrapper<TData>(syncFactory, asyncFactory);
    }

    public virtual void Initialize() { }
    public virtual void Load() { }
    public virtual void Unload() { }

    // ------------------------ ADD ------------------------

    public void Add(string key, string path, ResourceSourceType type = ResourceSourceType.ExternalFile)
    {
        Add<Stream>(key, path, null, type);
    }

    public void Add(string key, string path, CultureInfo? culture, ResourceSourceType type = ResourceSourceType.ExternalFile)
    {
        Add<Stream>(key, path, culture, type);
    }

    public void Add<TData>(string key, string path, ResourceSourceType type = ResourceSourceType.ExternalFile)
    {
        Add<TData>(key, path, null, type);
    }

    public void Add<TData>(string key, string path, CultureInfo? culture, ResourceSourceType type = ResourceSourceType.ExternalFile)
    {
        culture ??= CultureInfo.InvariantCulture;

        switch (type)
        {
            case ResourceSourceType.Content:
                AddContent<TData>(key, path, culture);
                break;
            case ResourceSourceType.EmbeddedResource:
                Add<TData, EmbeddedResourceSource>(key, path, culture);
                break;
            case ResourceSourceType.ExternalFile:
                Add<TData, LocalFileSource>(key, path, culture);
                break;
            default:
                throw new InvalidOperationException($"Unsupported ResourceSourceType: {type}");
        }
    }

    public void Add<TData, TLoader>(string key, string path, CultureInfo? culture = null)
        where TLoader : ResourceSourceBase
    {
        culture ??= CultureInfo.InvariantCulture;

        if (!_items.TryGetValue(culture, out var cultureDict))
            _items[culture] = cultureDict = new Dictionary<string, object>();

        if (cultureDict.ContainsKey(key))
            throw new InvalidOperationException($"Asset '{key}' for culture '{culture}' already exists.");

        Func<object?> factory = () =>
        {
            var loader = CreateLoader<TLoader>(path);
            if (!_registeredFactories.TryGetValue(typeof(TData), out var f))
                throw new InvalidOperationException($"No factory registered for type {typeof(TData).Name}.");

            var wrapper = (FactoryWrapper<TData>)f;
            return wrapper.CreateSync(this, loader);
        };

        cultureDict[key] = new ResourceEntry(factory);
    }

    private void AddContent<TData>(string key, string path, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        if (!_items.TryGetValue(culture, out var cultureDict))
            _items[culture] = cultureDict = new Dictionary<string, object>();

        if (cultureDict.ContainsKey(key))
            throw new InvalidOperationException($"Asset '{key}' for culture '{culture}' already exists.");

        Func<object?> factory = () =>
        {
            return _gameApplication.Content.Load<TData>(path);
        };

        cultureDict[key] = new ResourceEntry(factory);
    }

    public void AddAsync<TData, TLoader>(string key, string path, CultureInfo? culture = null)
        where TLoader : ResourceSourceBase
    {
        culture ??= CultureInfo.InvariantCulture;

        if (!_items.TryGetValue(culture, out var cultureDict))
            _items[culture] = cultureDict = new Dictionary<string, object>();

        if (cultureDict.ContainsKey(key))
            throw new InvalidOperationException($"Asset '{key}' for culture '{culture}' already exists.");

        Func<Task<object?>> factory = async () =>
        {
            var loader = CreateLoader<TLoader>(path);

            if (!_registeredFactories.TryGetValue(typeof(TData), out var f))
                throw new InvalidOperationException($"No factory registered for type {typeof(TData).Name}.");

            var wrapper = (FactoryWrapper<TData>)f;
            return await wrapper.CreateAsync(this, loader);
        };

        cultureDict[key] = new ResourceEntryAsync(factory);
    }

    public void AddAsync(string key, string path, ResourceSourceType type = ResourceSourceType.ExternalFile)
    {
        AddAsync<Stream>(key, path, null, type);
    }

    public void AddAsync(string key, string path, CultureInfo? culture = null, ResourceSourceType type = ResourceSourceType.ExternalFile)
    {
        AddAsync<Stream>(key, path, culture, type);
    }

    public void AddAsync<TData>(string key, string path, ResourceSourceType type = ResourceSourceType.ExternalFile)
    {
        AddAsync<TData>(key, path, null, type);
    }

    public void AddAsync<TData>(string key, string path, CultureInfo? culture = null, ResourceSourceType type = ResourceSourceType.ExternalFile)
    {
        culture ??= CultureInfo.InvariantCulture;

        switch (type)
        {
            case ResourceSourceType.Content:
                // ContentManager ist synchron, daher Task.FromResult
                AddAsyncContent<TData>(key, path, culture);
                break;

            case ResourceSourceType.EmbeddedResource:
                AddAsync<TData, EmbeddedResourceSource>(key, path, culture);
                break;

            case ResourceSourceType.ExternalFile:
                AddAsync<TData, LocalFileSource>(key, path, culture);
                break;

            default:
                throw new InvalidOperationException($"Unsupported ResourceSourceType: {type}");
        }
    }

    private void AddAsyncContent<TData>(string key, string path, CultureInfo culture)
    {
        if (!_items.TryGetValue(culture, out var cultureDict))
            _items[culture] = cultureDict = new Dictionary<string, object>();

        if (cultureDict.ContainsKey(key))
            throw new InvalidOperationException($"Asset '{key}' for culture '{culture}' already exists.");

        Func<Task<object?>> factory = () => Task.FromResult<object?>(_gameApplication.Content.Load<TData>(path));

        cultureDict[key] = new ResourceEntryAsync(factory);
    }

    // ------------------------ LOAD ------------------------

    public TData Load<TData>(string key, CultureInfo? culture = null)
    {
        return (TData)Load(key, culture);
    }

    public Stream LoadStream(string key, CultureInfo? culture = null)
    {
        return (Stream)Load(key, culture);
    }

    public object Load(string key, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;

        // 1) Versuche: exakte Kultur + alle Parent-Kulturen
        var currentCulture = culture;
        while (currentCulture != CultureInfo.InvariantCulture)
        {
            if (_items.TryGetValue(currentCulture, out var dict) &&
                dict.TryGetValue(key, out var entryObj) &&
                entryObj is ResourceEntry entry)
            {
                return entry.Value;
            }

            currentCulture = currentCulture.Parent; // z.B. de-DE → de
        }

        // 2) Fallback invariant
        if (_items.TryGetValue(CultureInfo.InvariantCulture, out var invariantDict) &&
            invariantDict.TryGetValue(key, out var fallbackObj) &&
            fallbackObj is ResourceEntry fallbackEntry)
        {
            return fallbackEntry.Value;
        }

        // 3) Fehler
        throw new KeyNotFoundException(
            $"Asset '{key}' not found for culture '{culture.Name}' or any parent culture.");
    }

    public async Task<TData?> LoadAsync<TData>(string key, CultureInfo? culture = null)
    {
        var obj = await LoadAsync(key, culture);
        return (TData?)obj;
    }

    public async Task<object?> LoadAsync(string key, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;

        if (_items.TryGetValue(culture, out var cultureDict) &&
            cultureDict.TryGetValue(key, out var entryObj))
        {
            switch (entryObj)
            {
                case ResourceEntryAsync asyncEntry:
                    return await asyncEntry.ValueAsync();
                case ResourceEntry syncEntry:
                    return await Task.FromResult(syncEntry.Value);
            }
        }

        // Fallback invariant
        if (_items.TryGetValue(CultureInfo.InvariantCulture, out var invariantDict) &&
            invariantDict.TryGetValue(key, out var fallbackObj))
        {
            switch (fallbackObj)
            {
                case ResourceEntryAsync asyncFallback:
                    return await asyncFallback.ValueAsync();
                case ResourceEntry syncFallback:
                    return await Task.FromResult(syncFallback.Value);
            }
        }

        throw new KeyNotFoundException($"Asset '{key}' not found for culture '{culture.Name}'.");
    }

    // ------------------------ ADD & LOAD ------------------------

    /// <summary>
    /// Fügt einen Asset hinzu und lädt ihn sofort synchron.
    /// </summary>
    public TData AddAndLoad<TData>(string key, string path, CultureInfo? culture = null, ResourceSourceType type = ResourceSourceType.Content)
    {
        Add<TData>(key, path, culture, type);
        return Load<TData>(key, culture);
    }

    /// <summary>
    /// Fügt einen Asset hinzu und lädt ihn sofort asynchron.
    /// </summary>
    public async Task<TData?> AddAndLoadAsync<TData>(string key, string path, CultureInfo? culture = null, ResourceSourceType type = ResourceSourceType.Content)
    {
        await Task.Yield(); // optional: minimaler async Kontext
        AddAsync<TData>(key, path, culture, type);
        return await LoadAsync<TData>(key, culture);
    }

    // ------------------------ WRAPPER ------------------------

    private class FactoryWrapper<TData>
    {
        public Func<AssetStore, ResourceSourceBase, TData>? SyncFactory { get; }
        public Func<AssetStore, ResourceSourceBase, Task<TData>>? AsyncFactory { get; }

        public FactoryWrapper(
            Func<AssetStore, ResourceSourceBase, TData>? syncFactory,
            Func<AssetStore, ResourceSourceBase, Task<TData>>? asyncFactory)
        {
            SyncFactory = syncFactory;
            AsyncFactory = asyncFactory;
        }

        public TData CreateSync(AssetStore app, ResourceSourceBase loader)
        {
            if (SyncFactory == null)
                throw new InvalidOperationException($"No synchronous factory registered for type {typeof(TData).Name}.");
            return SyncFactory(app, loader);
        }

        public Task<TData> CreateAsync(AssetStore app, ResourceSourceBase loader)
        {
            if (AsyncFactory != null)
                return AsyncFactory(app, loader);
            if (SyncFactory != null)
                return Task.FromResult(SyncFactory(app, loader));
            throw new InvalidOperationException($"No factory registered for type {typeof(TData).Name}.");
        }
    }

    private class ResourceEntry
    {
        private readonly Func<object?> _factory;
        private object? _value;
        private bool _loaded;

        public ResourceEntry(Func<object?> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public object Value
        {
            get
            {
                if (!_loaded)
                {
                    _value = _factory();
                    _loaded = true;
                }
                return _value!;
            }
        }

        public bool IsLoaded => _loaded;
    }

    private class ResourceEntryAsync
    {
        private readonly Func<Task<object?>> _factory;
        private Task<object?>? _loadingTask;

        public ResourceEntryAsync(Func<Task<object?>> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public Task<object?> ValueAsync()
        {
            if (_loadingTask == null)
                _loadingTask = _factory();
            return _loadingTask;
        }

        public bool IsLoaded => _loadingTask?.IsCompleted ?? false;
    }
}