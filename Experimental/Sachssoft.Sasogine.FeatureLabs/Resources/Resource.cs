using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.FeatureLabs.Resources;

public class Resource : IResourceTreeEntry, ITypedResource, IDisposable
{
    private readonly object _lock = new();
    private Stream? _stream;
    private object? _instance;
    private bool _isLoaded;
    private bool _disposed;

    // Parameterless constructor (für z.B. Deserialisierung)
    public Resource()
    {
        Id = Guid.NewGuid().ToString();
        TargetType = typeof(object);
        Properties = new PropertySet();
        Children = ImmutableHashSet<IResourceTreeEntry>.Empty;
    }

    [SetsRequiredMembers]
    public Resource(string id, Type targetType, PropertySet properties, IEnumerable<Resource>? children = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        Children = (children ?? Enumerable.Empty<Resource>())
            .Cast<IResourceTreeEntry>()
            .ToImmutableHashSet();
    }

    // required properties mit init
    public required string Id { get; init; }
    public string? Class { get; init; }
    public required Type TargetType { get; init; }
    public required PropertySet Properties { get; init; }
    public required IReadOnlySet<IResourceTreeEntry> Children { get; init; }

    public string? File { get; init; }
    public string? Content { get; init; }

    public bool IsLoaded => _isLoaded;

    // Lazy Stream (immer neu öffnen, kein Caching)
    public Stream GetStream(ResourceStore store)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(File))
            throw new InvalidOperationException("Resource file is not set.");

        return store.Source.OpenStream(File, store.RootPath)
               ?? throw new InvalidOperationException($"Failed to open resource stream '{File}'.");
    }

    // Thread-safe Load
    public void Load(ResourceStore store)
    {
        ThrowIfDisposed();

        if (_isLoaded)
            return;

        lock (_lock)
        {
            if (_isLoaded)
                return;

            _instance = store.Registry.Create(store, this)
                        ?? throw new InvalidOperationException($"Failed to create instance for resource '{Id}'.");

            _isLoaded = true;
        }
    }

    public void Reload(ResourceStore store)
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            // Alte Instanz freigeben
            if (_instance is IDisposable disposable)
                disposable.Dispose();

            _instance = store.Registry.Create(store, this)
                        ?? throw new InvalidOperationException($"Failed to create instance for resource '{Id}'.");

            _isLoaded = true;
        }
    }

    public T GetInstance<T>()
    {
        ThrowIfDisposed();

        if (!_isLoaded)
            throw new InvalidOperationException($"Resource '{Id}' is not loaded. Call Load() first.");

        if (_instance is null)
            throw new InvalidOperationException($"Resource '{Id}' instance was not created during Load().");

        if (_instance is not T typed)
            throw new InvalidCastException(
                $"Resource instance is of type '{_instance.GetType().FullName}', not '{typeof(T).FullName}'.");

        return typed;
    }

    public IResourceTreeEntry? GetChild(string id) =>
        Children.FirstOrDefault(c => string.Equals(c.Id, id, StringComparison.Ordinal));

    public IEnumerable<IResourceTreeEntry> GetChildrenByClass(string className) =>
        Children.Where(c => string.Equals(c.Class, className, StringComparison.Ordinal));

    public IEnumerable<IResourceTreeEntry> GetDescendants()
    {
        foreach (var child in Children)
        {
            yield return child;

            foreach (var descendant in child.GetDescendants())
                yield return descendant;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_lock)
        {
            if (_disposed)
                return;

            _stream?.Dispose();
            _stream = null;

            if (_instance is IDisposable disposable)
                disposable.Dispose();

            _instance = null;
            _isLoaded = false;

            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Resource));
    }
}