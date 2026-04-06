using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sachssoft.Sasogine.Resources;

public abstract class ResourceStore : IResourceContainer
{
    private readonly IGameApplication _application;
    private readonly ResourceRegistry _resourceRegistry;
    private readonly ResourceFileSource _source;
    private readonly string? _rootPath;
    private readonly IReadOnlyList<IResourceEntry> _entries;

    protected ResourceStore(IGameApplication application, IEnumerable<IResourceEntry> entries, ResourceFileSource source, string? rootPath = null)
    {
        _application = application;
        _source = source;
        _rootPath = rootPath;
        _resourceRegistry = CreateRegistry();
        _entries = (new List<IResourceEntry>(entries)).AsReadOnly(); // Immutable !!
    }

    public IGameApplication Application => _application;

    public ResourceRegistry Registry => _resourceRegistry;

    public ResourceFileSource Source => _source;

    public string? RootPath => _rootPath;

    protected IReadOnlyList<IResourceEntry> Entries => _entries;

    public bool TryGetResource<T>(string id, [MaybeNullWhen(false)] out T value, ResourceLookupOptions options = ResourceLookupOptions.None)
    {
        value = default;
        Resource? resourceFound = null;

        // Entry holen (lokal oder hierarchisch)
        bool found = (options & ResourceLookupOptions.IncludeHierarchy) != 0
            ? TryGetTreeEntry<Resource>(id, out resourceFound)
            : TryGetEntry<Resource>(id, out resourceFound);

        if (!found || resourceFound == null)
            return false;

        // Optional: Reload
        if ((options & ResourceLookupOptions.Reload) != 0)
        {
            resourceFound.Reload(this);
        }
        else if (!resourceFound.IsLoaded)
        {
            resourceFound.Load(this);
        }

        // Typprüfung ohne Exceptions
        if (resourceFound is ITypedResource typedResource)
        {
            if (typedResource.TargetType == typeof(T))
            {
                value = resourceFound.GetInstance<T>();
                return true;
            }
            else
            {
                // Typ stimmt nicht
                return false;
            }
        }
        else
        {
            value = resourceFound.GetInstance<T>();
            return true;
        }
    }

    public bool TryGetEntry<T>(string? id, [MaybeNullWhen(false)] out T entry)
        where T : class, IResourceEntry
    {
        entry = _entries.OfType<T>()
                        .FirstOrDefault(s =>
                            string.Equals(s.Id, id, StringComparison.Ordinal)
                        );

        return entry != null;
    }

    public bool TryGetEntry<T>(Type targetType, string? id, [MaybeNullWhen(false)] out T entry)
        where T : class, IResourceEntry
    {
        entry = _entries.OfType<T>()
                        .FirstOrDefault(s =>
                            s is ITypedResource ts &&
                            ts.TargetType == typeof(T) &&
                            string.Equals(s.Id, id, StringComparison.Ordinal)
                        );

        return entry != null;
    }

    public bool TryGetTreeEntry<T>(string? id, [MaybeNullWhen(false)] out T entry)
        where T : class, IResourceEntry
    {
        foreach (var e in _entries)
        {
            if (TryFindRecursive(e, id, out entry))
                return true;
        }

        entry = null;
        return false;
    }

    public bool TryGetTreeEntry<T>(Type targetType, string? id, [MaybeNullWhen(false)] out T entry)
        where T : class, IResourceEntry
    {
        foreach (var e in _entries)
        {
            if (TryFindRecursive(e, targetType, id, out entry))
                return true;
        }

        entry = null;
        return false;
    }

    private bool TryFindRecursive<T>(IResourceEntry current, string? id, [MaybeNullWhen(false)] out T result)
        where T : class, IResourceEntry
    {
        result = null;

        if (current is T typed &&
            string.Equals(current.Id, id, StringComparison.Ordinal))
        {
            result = typed;
            return true;
        }

        // 🔹 Check children
        if (current is IResourceTreeEntry tree)
        {
            foreach (var child in tree.Children)
            {
                if (TryFindRecursive(child, id, out result))
                    return true;
            }
        }

        return false;
    }

    private bool TryFindRecursive<T>(IResourceEntry current, Type targetType, string? id, [MaybeNullWhen(false)] out T result)
        where T : class, IResourceEntry
    {
        result = null;

        if (current is not ITypedResource tr)
            return false;

        if (current is T typed &&
            tr.TargetType == targetType &&
            string.Equals(current.Id, id, StringComparison.Ordinal))
        {
            result = typed;
            return true;
        }

        // 🔹 Check children
        if (current is IResourceTreeEntry tree)
        {
            foreach (var child in tree.Children)
            {
                if (TryFindRecursive(child, targetType, id, out result))
                    return true;
            }
        }

        return false;
    }

    protected virtual ResourceRegistry CreateRegistry() => new ResourceRegistry();
}
