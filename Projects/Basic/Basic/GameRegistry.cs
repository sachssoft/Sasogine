using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides an AOT-friendly registry for creating definitions and engine
/// object instances.
/// </summary>
/// <typeparam name="TKey">The type of key used to identify registry entries.</typeparam>
/// <typeparam name="TDefinition">The base type of definitions managed by the registry.</typeparam>
/// <typeparam name="TObject">The base type of engine objects managed by the registry.</typeparam>
public class GameRegistry<TKey, TDefinition, TObject> : IGameRegistry
    where TKey : notnull, IGameRegistryKey
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    private readonly object _lock = new();
    private readonly Dictionary<IGameRegistryKey, IGameRegistryEntry> _entriesByKey = [];
    private readonly Dictionary<Type, IGameRegistryEntry> _entriesByDefinitionType = [];
    private readonly Dictionary<Type, IGameRegistryEntry> _entriesByObjectType = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GameRegistry{TKey, TDefinition, TObject}"/>
    /// class using exact definition type matching.
    /// </summary>
    public GameRegistry() : this(DefinitionMatchMode.Exact)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameRegistry{TKey, TDefinition, TObject}"/> class.
    /// </summary>
    /// <param name="definitionMatchMode">The mode used to match definition types.</param>
    public GameRegistry(DefinitionMatchMode definitionMatchMode)
    {
        DefinitionMatchMode = definitionMatchMode;
    }

    /// <summary>
    /// Gets how definition types are matched when resolving registry entries.
    /// </summary>
    public DefinitionMatchMode DefinitionMatchMode { get; }

    /// <summary>
    /// Gets a snapshot of all registered entries.
    /// </summary>
    protected IReadOnlyList<IGameRegistryEntry> GetEntries()
    {
        lock (_lock)
        {
            return _entriesByKey.Values.ToArray();
        }
    }

    /// <summary>
    /// Registers a concrete definition type and its corresponding concrete
    /// engine object type using the specified registry key.
    /// </summary>
    /// <typeparam name="TConcreteDefinition">
    /// The concrete definition type to register.
    /// </typeparam>
    /// <typeparam name="TConcreteObject">
    /// The concrete engine object type to register.
    /// </typeparam>
    /// <param name="key">
    /// The key used to identify the registry entry.
    /// </param>
    /// <param name="definitionFactory">
    /// The factory used to create definition instances.
    /// </param>
    /// <param name="objectFactory">
    /// The factory used to create engine object instances from definitions.
    /// </param>
    public void Register<TConcreteDefinition, TConcreteObject>(
        TKey key,
        Func<TConcreteDefinition> definitionFactory,
        Func<TConcreteDefinition, TConcreteObject> objectFactory)
        where TConcreteDefinition : class, TDefinition
        where TConcreteObject : class, TObject
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definitionFactory);
        ArgumentNullException.ThrowIfNull(objectFactory);

        IGameRegistryEntry entry = CreateEntry(
            key,
            typeof(TConcreteDefinition),
            typeof(TConcreteObject),
            () => definitionFactory(),
            definition => objectFactory((TConcreteDefinition)definition));

        RegisterEntry(entry);
    }

    /// <summary>
    /// Determines whether the registry contains the definition type represented
    /// by <typeparamref name="TDefinition"/>.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the definition type is registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsDefinitionRegistered() => IsDefinitionRegistered(typeof(TDefinition));

    /// <summary>
    /// Determines whether the specified definition type can be resolved.
    /// </summary>
    /// <param name="definitionType">The definition type to check.</param>
    /// <returns>
    /// <see langword="true"/> if the definition type can be resolved;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsDefinitionRegistered(Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        lock (_lock)
        {
            if (_entriesByDefinitionType.ContainsKey(definitionType))
                return true;

            if (DefinitionMatchMode == DefinitionMatchMode.Exact)
                return false;

            foreach (IGameRegistryEntry entry in _entriesByDefinitionType.Values)
            {
                if (entry.DefinitionType.IsAssignableFrom(definitionType))
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Determines whether the registry contains the object type represented
    /// by <typeparamref name="TObject"/>.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the object type is registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsObjectRegistered() => IsObjectRegistered(typeof(TObject));

    /// <summary>
    /// Determines whether the specified engine object type can be resolved.
    /// </summary>
    /// <param name="objectType">The engine object type to check.</param>
    /// <returns>
    /// <see langword="true"/> if the object type can be resolved;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsObjectRegistered(Type objectType)
    {
        ArgumentNullException.ThrowIfNull(objectType);

        lock (_lock)
        {
            if (_entriesByObjectType.ContainsKey(objectType))
                return true;

            foreach (IGameRegistryEntry entry in _entriesByObjectType.Values)
            {
                if (objectType.IsAssignableFrom(entry.ObjectType))
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Creates a definition using the registered definition type represented
    /// by <typeparamref name="TDefinition"/>.
    /// </summary>
    /// <returns>The created definition.</returns>
    public TDefinition CreateDefinition() => CreateDefinition(typeof(TDefinition));

    /// <summary>
    /// Creates a definition for the specified definition type.
    /// </summary>
    /// <param name="definitionType">The definition type used to locate the registry entry.</param>
    /// <returns>The created definition.</returns>
    protected virtual TDefinition CreateDefinition(Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        if (TryGetDefinitionEntry(definitionType, out IGameRegistryEntry? entry))
            return (TDefinition)entry.CreateDefinition();

        throw new KeyNotFoundException(
            $"Definition type '{definitionType.FullName}' is not registered.");
    }

    /// <summary>
    /// Creates a definition associated with the specified registry key.
    /// </summary>
    /// <param name="key">The registry key used to locate the registry entry.</param>
    /// <returns>The created definition.</returns>
    public TDefinition CreateDefinition(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (TryGetKeyEntry(key, out IGameRegistryEntry? entry))
            return (TDefinition)entry.CreateDefinition();

        throw new KeyNotFoundException(
            $"Registry key '{key}' is not registered.");
    }

    /// <summary>
    /// Attempts to create a definition for the specified definition type.
    /// </summary>
    /// <param name="definitionType">The definition type used to locate the registry entry.</param>
    /// <param name="definition">
    /// When this method returns <see langword="true"/>, contains the created
    /// definition; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the definition could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool TryCreateDefinition(Type definitionType, [NotNullWhen(true)] out TDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        if (!TryGetDefinitionEntry(definitionType, out IGameRegistryEntry? entry))
        {
            definition = null;
            return false;
        }

        definition = (TDefinition)entry.CreateDefinition();
        return true;
    }

    /// <summary>
    /// Attempts to create a definition associated with the specified registry key.
    /// </summary>
    /// <param name="key">The registry key used to locate the registry entry.</param>
    /// <param name="definition">
    /// When this method returns <see langword="true"/>, contains the created
    /// definition; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the definition could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryCreateDefinition(TKey key, [NotNullWhen(true)] out TDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!TryGetKeyEntry(key, out IGameRegistryEntry? entry))
        {
            definition = null;
            return false;
        }

        definition = (TDefinition)entry.CreateDefinition();
        return true;
    }

    /// <summary>
    /// Creates an engine object compatible with the specified object type
    /// using the provided definition.
    /// </summary>
    /// <param name="objectType">The requested engine object type.</param>
    /// <param name="definition">The definition used to create the engine object.</param>
    /// <returns>The created engine object.</returns>
    protected virtual TObject Create(Type objectType, IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(definition);

        if (TryGetObjectEntry(objectType, definition.GetType(), out IGameRegistryEntry? entry))
            return (TObject)entry.CreateObject(definition);

        throw new KeyNotFoundException(
            $"No registry entry was found for object type '{objectType.FullName}' " +
            $"and definition type '{definition.GetType().FullName}'.");
    }

    /// <summary>
    /// Attempts to create an engine object compatible with the specified object
    /// type using the provided definition.
    /// </summary>
    /// <param name="objectType">The requested engine object type.</param>
    /// <param name="definition">The definition used to create the engine object.</param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, contains the created
    /// engine object; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the engine object could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool TryCreate(Type objectType, IDefinition definition, [NotNullWhen(true)] out TObject? instance)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(definition);

        if (!TryGetObjectEntry(objectType, definition.GetType(), out IGameRegistryEntry? entry))
        {
            instance = null;
            return false;
        }

        instance = (TObject)entry.CreateObject(definition);
        return true;
    }

    /// <summary>
    /// Creates an engine object using the registry entry associated with the
    /// specified definition.
    /// </summary>
    /// <param name="definition">The definition used to locate the registry entry.</param>
    /// <returns>The created engine object.</returns>
    public TObject CreateFromDefinition(IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (TryGetDefinitionEntry(definition.GetType(), out IGameRegistryEntry? entry))
            return (TObject)entry.CreateObject(definition);

        throw new KeyNotFoundException(
            $"Definition type '{definition.GetType().FullName}' is not registered.");
    }

    /// <summary>
    /// Attempts to create an engine object using the registry entry associated
    /// with the specified definition.
    /// </summary>
    /// <param name="definition">The definition used to locate the registry entry.</param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, contains the created
    /// engine object; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the engine object could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryCreateFromDefinition(IDefinition definition, [NotNullWhen(true)] out TObject? instance)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!TryGetDefinitionEntry(definition.GetType(), out IGameRegistryEntry? entry))
        {
            instance = null;
            return false;
        }

        instance = (TObject)entry.CreateObject(definition);
        return true;
    }

    /// <summary>
    /// Creates a registry entry using the specified key, concrete types,
    /// and factories.
    /// </summary>
    /// <param name="key">The registry key.</param>
    /// <param name="definitionType">The concrete definition type.</param>
    /// <param name="objectType">The concrete engine object type.</param>
    /// <param name="definitionFactory">
    /// The factory used to create definitions.
    /// </param>
    /// <param name="objectFactory">
    /// The factory used to create engine objects from definitions.
    /// </param>
    /// <returns>The created registry entry.</returns>
    protected virtual IGameRegistryEntry CreateEntry(
        IGameRegistryKey key,
        Type definitionType,
        Type objectType,
        Func<IDefinition> definitionFactory,
        Func<IDefinition, IEngineObject> objectFactory)
    {
        return new GameRegistryEntry(
            key,
            definitionType,
            objectType,
            definitionFactory,
            objectFactory);
    }

    /// <summary>
    /// Registers the specified registry entry.
    /// </summary>
    /// <param name="entry">The registry entry to register.</param>
    protected virtual void RegisterEntry(IGameRegistryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_lock)
        {
            Console.WriteLine("=== RegisterEntry ===");
            Console.WriteLine($"Key:            {entry.Key}");
            Console.WriteLine($"KeyType:        {entry.KeyType.FullName}");
            Console.WriteLine($"DefinitionType: {entry.DefinitionType.FullName}");
            Console.WriteLine($"ObjectType:     {entry.ObjectType.FullName}");
            Console.WriteLine();

            if (_entriesByKey.ContainsKey(entry.Key))
            {
                throw new InvalidOperationException(
                    $"Registry key '{entry.Key}' is already registered.");
            }

            if (_entriesByDefinitionType.ContainsKey(entry.DefinitionType))
            {
                throw new InvalidOperationException(
                    $"Definition type '{entry.DefinitionType.FullName}' is already registered.");
            }

            if (_entriesByObjectType.ContainsKey(entry.ObjectType))
            {
                throw new InvalidOperationException(
                    $"Object type '{entry.ObjectType.FullName}' is already registered.");
            }

            _entriesByKey.Add(entry.Key, entry);
            _entriesByDefinitionType.Add(entry.DefinitionType, entry);
            _entriesByObjectType.Add(entry.ObjectType, entry);
        }
    }

    private bool TryGetKeyEntry(IGameRegistryKey key, [NotNullWhen(true)] out IGameRegistryEntry? entry)
    {
        lock (_lock)
        {
            return _entriesByKey.TryGetValue(key, out entry);
        }
    }

    private bool TryGetDefinitionEntry(Type definitionType, [NotNullWhen(true)] out IGameRegistryEntry? entry)
    {
        lock (_lock)
        {
            if (_entriesByDefinitionType.TryGetValue(definitionType, out entry))
                return true;

            if (DefinitionMatchMode == DefinitionMatchMode.Exact)
            {
                entry = null;
                return false;
            }

            IGameRegistryEntry? best = null;

            foreach (IGameRegistryEntry candidate in _entriesByDefinitionType.Values)
            {
                if (!candidate.DefinitionType.IsAssignableFrom(definitionType))
                    continue;

                if (best is null || best.DefinitionType.IsAssignableFrom(candidate.DefinitionType))
                    best = candidate;
            }

            entry = best;
            return entry is not null;
        }
    }

    private bool TryGetObjectEntry(
        Type objectType,
        Type definitionType,
        [NotNullWhen(true)] out IGameRegistryEntry? entry)
    {
        lock (_lock)
        {
            if (_entriesByDefinitionType.TryGetValue(definitionType, out IGameRegistryEntry? exact))
            {
                if (objectType.IsAssignableFrom(exact.ObjectType))
                {
                    entry = exact;
                    return true;
                }

                entry = null;
                return false;
            }

            if (DefinitionMatchMode == DefinitionMatchMode.Exact)
            {
                entry = null;
                return false;
            }

            IGameRegistryEntry? best = null;

            foreach (IGameRegistryEntry candidate in _entriesByDefinitionType.Values)
            {
                if (!candidate.DefinitionType.IsAssignableFrom(definitionType))
                    continue;

                if (!objectType.IsAssignableFrom(candidate.ObjectType))
                    continue;

                if (best is null || best.DefinitionType.IsAssignableFrom(candidate.DefinitionType))
                    best = candidate;
            }

            entry = best;
            return entry is not null;
        }
    }

    void IGameRegistry.Register(
        IGameRegistryKey key,
        Func<IDefinition> definitionFactory,
        Func<IDefinition, IEngineObject> objectFactory)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definitionFactory);
        ArgumentNullException.ThrowIfNull(objectFactory);

        if (key is not TKey typedKey)
        {
            throw new ArgumentException(
                $"Registry key type '{key.GetType().FullName}' is not compatible with '{typeof(TKey).FullName}'.",
                nameof(key));
        }

        IDefinition definition = definitionFactory()
            ?? throw new InvalidOperationException(
                "The definition factory returned null.");

        if (definition is not TDefinition)
        {
            throw new InvalidOperationException(
                $"Definition type '{definition.GetType().FullName}' is not compatible with '{typeof(TDefinition).FullName}'.");
        }

        IEngineObject instance = objectFactory(definition)
            ?? throw new InvalidOperationException(
                "The object factory returned null.");

        if (instance is not TObject)
        {
            throw new InvalidOperationException(
                $"Object type '{instance.GetType().FullName}' is not compatible with '{typeof(TObject).FullName}'.");
        }

        IGameRegistryEntry entry = CreateEntry(
            typedKey,
            definition.GetType(),
            instance.GetType(),
            definitionFactory,
            objectFactory);

        RegisterEntry(entry);
    }

    bool IGameRegistry.IsDefinitionRegistered(Type definitionType)
        => IsDefinitionRegistered(definitionType);

    bool IGameRegistry.IsObjectRegistered(Type objectType)
        => IsObjectRegistered(objectType);

    IDefinition IGameRegistry.CreateDefinition(Type definitionType)
        => CreateDefinition(definitionType);

    IDefinition IGameRegistry.CreateDefinition(IGameRegistryKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!TryGetKeyEntry(key, out IGameRegistryEntry? entry))
            throw new KeyNotFoundException($"Registry key '{key}' is not registered.");

        return entry.CreateDefinition();
    }

    bool IGameRegistry.TryCreateDefinition(
        Type definitionType,
        [NotNullWhen(true)] out IDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        if (!TryGetDefinitionEntry(definitionType, out IGameRegistryEntry? entry))
        {
            definition = null;
            return false;
        }

        definition = entry.CreateDefinition();
        return true;
    }

    bool IGameRegistry.TryCreateDefinition(
        IGameRegistryKey key,
        [NotNullWhen(true)] out IDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!TryGetKeyEntry(key, out IGameRegistryEntry? entry))
        {
            definition = null;
            return false;
        }

        definition = entry.CreateDefinition();
        return true;
    }

    IEngineObject IGameRegistry.Create(Type objectType, IDefinition definition)
        => Create(objectType, definition);

    bool IGameRegistry.TryCreate(
        Type objectType,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        if (TryCreate(objectType, definition, out TObject? typedInstance))
        {
            instance = typedInstance;
            return true;
        }

        instance = null;
        return false;
    }

    IEngineObject IGameRegistry.CreateFromDefinition(IDefinition definition)
        => CreateFromDefinition(definition);

    bool IGameRegistry.TryCreateFromDefinition(
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        if (TryCreateFromDefinition(definition, out TObject? typedInstance))
        {
            instance = typedInstance;
            return true;
        }

        instance = null;
        return false;
    }
}