namespace Sachssoft.Sasogine.FeatureLabs.Resources;

public sealed class ResourceFileSource
{
    private const string ENTRY_LOCAL = "Local";
    private const string ENTRY_EMBEDDED = "Embedded";

    private static readonly Dictionary<string, Func<IFileLoader>> s_registeredTypes = new(StringComparer.OrdinalIgnoreCase);

    private readonly Func<IFileLoader> _factory;

    public static ResourceFileSource Local => From(ENTRY_LOCAL);
    public static ResourceFileSource Embedded => From(ENTRY_EMBEDDED);

    static ResourceFileSource()
    {
        Register<LocalFileLoader>(ENTRY_LOCAL);
        Register<EmbeddedResourceLoader>(ENTRY_EMBEDDED);
    }

    private ResourceFileSource(string name, Func<IFileLoader> factory)
    {
        Name = name;
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public string Name { get; }

    // 🔹 Sync
    public Stream OpenStream(string filePath, string? rootPath = null)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("filePath is null or empty");

        var loader = _factory();

        string path = rootPath != null
            ? Path.Combine(rootPath, filePath)
            : filePath;

        loader.FilePath = path;

        return loader.GetStream();
    }

    // 🔹 Async
    public Task<Stream> OpenStreamAsync(string filePath, string? rootPath = null)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("filePath is null or empty");

        var loader = _factory();

        string path = rootPath != null
            ? Path.Combine(rootPath, filePath)
            : filePath;

        loader.FilePath = path;

        return loader.GetStreamAsync();
    }

    // 🔹 Direkt erstellen (ohne Register)
    public static ResourceFileSource Create<T>(string name)
        where T : LoaderBase, IFileLoader, new()
    {
        return new ResourceFileSource(name, () => new T());
    }

    // 🔹 Registrierung (Plugins / Erweiterung)
    public static void Register<T>(string name)
        where T : LoaderBase, IFileLoader, new()
    {
        s_registeredTypes[name] = () => new T();
    }

    // 🔹 Aus Registry holen
    public static ResourceFileSource From(string name)
    {
        if (!s_registeredTypes.TryGetValue(name, out var factory))
            throw new InvalidOperationException($"ResourceFileSource '{name}' not registered.");

        return new ResourceFileSource(name, factory);
    }

    // z.B  ResourceFileSource local = "Local";
    public static implicit operator ResourceFileSource(string name)
    {
        return From(name);
    }
}