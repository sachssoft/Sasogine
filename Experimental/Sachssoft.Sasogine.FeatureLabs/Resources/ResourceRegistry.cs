using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Resources.Factories;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Resources;

public class ResourceRegistry
{
    // Key: Type direkt → stabil und refactor-sicher
    private readonly Dictionary<Type, Func<ResourceStore, IResourceEntry, object?>> _typeFactories = new();
    private static readonly string[] s_defaultNamespaces = new[]
    {
        "Microsoft.Xna.Framework",
        "Microsoft.Xna.Framework.Audio",
        "Microsoft.Xna.Framework.Graphics",
        "Microsoft.Xna.Framework.Media",
        "Sachssoft.Sasogine.Presentation.Rendering",
        "Sachssoft.Sasogine.Presentation.Styling"
    };

    public ResourceRegistry()
    {
        RegisterDefaults();
    }

    public IEnumerable<Type> RegisteredTypes => _typeFactories.Keys;

    public void Register<T, TFactory>()
        where TFactory : ITypeFactory<T, Resource>, new()
    {
        var type = typeof(T);

        if (_typeFactories.ContainsKey(type))
            throw new InvalidOperationException($"A factory for type '{type.FullName}' is already registered.");

        var factory = new TFactory();
        _typeFactories[type] = (store, entry) =>
        {
            if (entry is not Resource res)
                throw new InvalidOperationException("Expected Resource instance.");

            return factory.Create(store, res);
        };
    }

    public bool ContainsType(string name, [MaybeNullWhen(false)] out Type targetType, string? @namespace = null)
    {
        foreach (var type in _typeFactories.Keys)
        {
            string typeNamespace = type.Namespace ?? "";

            if ((@namespace == null && Array.Exists(s_defaultNamespaces, ns => ns == typeNamespace)) ||
                (@namespace != null && typeNamespace == @namespace))
            {
                if (type.Name == name)
                {
                    targetType = type;
                    return true;
                }
            }
        }

        targetType = null;
        return false;
    }

    // Create über Name + Skin + Resource
    public object Create(string name, ResourceStore store, Resource resource)
    {
        foreach (var type in _typeFactories.Keys)
        {
            if (type.Name != name)
                continue;

            var factory = _typeFactories[type];
            var result = factory.Invoke(store, resource)
                         ?? throw new InvalidOperationException($"Factory for '{type.FullName}' returned null.");

            if (!type.IsAssignableFrom(result.GetType()))
                throw new InvalidOperationException($"Factory for '{type.FullName}' returned invalid type '{result.GetType().FullName}'.");

            return result;
        }

        throw new InvalidOperationException($"No factory registered for type '{name}'.");
    }

    public T Create<T>(ResourceStore store, IResourceEntry entry)
        where T : class
    {
        var type = typeof(T);

        if (!_typeFactories.TryGetValue(type, out var factory))
            throw new InvalidOperationException($"No factory registered for type '{type.FullName}'.");

        var result = factory.Invoke(store, entry)
                     ?? throw new InvalidOperationException($"Factory for '{type.FullName}' returned null.");

        if (!(result is T typedResult))
            throw new InvalidOperationException($"Factory for '{type.FullName}' returned invalid type '{result.GetType().FullName}'.");

        return typedResult;
    }

    public object Create(ResourceStore store, Resource resource)
    {
        var type = resource.TargetType;

        if (!_typeFactories.TryGetValue(type, out var factory))
            throw new InvalidOperationException($"No factory registered for type '{type.FullName}'.");

        var result = factory.Invoke(store, (IResourceEntry)resource)
                     ?? throw new InvalidOperationException($"Factory for '{type.FullName}' returned null.");

        if (!type.IsAssignableFrom(result.GetType()))
            throw new InvalidOperationException($"Factory for '{type.FullName}' returned invalid type '{result.GetType().FullName}'.");

        return result;
    }

    public bool TryCreate(string name, ResourceStore store, Resource resource, [MaybeNullWhen(false)] out object instance)
    {
        instance = null;

        foreach (var type in _typeFactories.Keys)
        {
            if (type.Name != name)
                continue;

            var factory = _typeFactories[type];
            var result = factory.Invoke(store, resource);

            if (result == null || !type.IsAssignableFrom(result.GetType()))
                return false;

            instance = result;
            return true;
        }

        return false;
    }

    public bool TryCreate(ResourceStore store, Resource resource, [MaybeNullWhen(false)] out object? instance)
    {
        instance = null;
        var type = resource.TargetType;

        if (!_typeFactories.TryGetValue(type, out var factory))
            return false;

        var result = factory.Invoke(store, (IResourceEntry)resource);
        if (result == null || !type.IsAssignableFrom(result.GetType()))
            return false;

        instance = result;
        return true;
    }

    public bool TryCreate<T>(ResourceStore store, Resource resource, [MaybeNullWhen(false)] out T instance)
        where T : class
    {
        instance = null;
        var type = typeof(T);

        if (!_typeFactories.TryGetValue(type, out var factory))
            return false;

        var result = factory.Invoke(store, resource);
        if (!(result is T typedResult))
            return false;

        instance = typedResult;
        return true;
    }

    protected virtual void RegisterDefaults()
    {
        Register<Bounds, BoundsFactory>();
        Register<Color, ColorFactory>();
        Register<float, FloatFactory>();
        Register<Insets, InsetsFactory>();
        Register<int, IntegerFactory>();
        Register<Texture2D, Texture2DFactory>();
        Register<TextureAtlas, TextureAtlasFactory>();
        Register<TextureAtlasSet, TextureAtlasSetFactory>();
    }
}