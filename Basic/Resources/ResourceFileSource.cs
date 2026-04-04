using Sachssoft.Sasogine.Resources.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources;

public sealed class ResourceFileSource
{
    private static readonly Dictionary<string, Func<IFileLoader>> _registeredTypes = new(StringComparer.OrdinalIgnoreCase);

    private readonly Func<IFileLoader> _factory;

    static ResourceFileSource()
    {
        Register<LocalFileLoader>("Local");
        Register<EmbeddedResourceLoader>("Embedded");
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
        _registeredTypes[name] = () => new T();
    }

    // 🔹 Aus Registry holen
    public static ResourceFileSource From(string name)
    {
        if (!_registeredTypes.TryGetValue(name, out var factory))
            throw new InvalidOperationException($"ResourceFileSource '{name}' not registered.");

        return new ResourceFileSource(name, factory);
    }

    // z.B  ResourceFileSource local = "Local";
    public static implicit operator ResourceFileSource(string name)
    {
        return From(name);
    }
}