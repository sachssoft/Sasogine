using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Provides an AOT-friendly registry for creating engine object instances
/// from engine object definitions using arbitrary identifiers.
/// </summary>
/// <remarks>
/// Each registered factory requires an engine object definition when creating
/// an instance. Factories may be indexed by strings, integers, enumeration
/// values, types, or other non-null key types.
/// </remarks>
public class GameRegistry
{
    private readonly Dictionary<Type, IFactoryMap> _factoryMaps = new();

    /// <summary>
    /// Registers a factory using the specified identifier.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of identifier used to register the factory.
    /// </typeparam>
    /// <typeparam name="TDefinition">
    /// The type of definition accepted by the factory.
    /// </typeparam>
    /// <typeparam name="TObject">
    /// The type of engine object created by the factory.
    /// </typeparam>
    /// <param name="key">
    /// The identifier used to resolve the factory.
    /// </param>
    /// <param name="factory">
    /// The factory used to create the engine object.
    /// </param>
    public void Register<TKey, TDefinition, TObject>(
        TKey key,
        Func<TDefinition, TObject> factory)
        where TKey : notnull
        where TDefinition : class, IEngineObjectDefinition
        where TObject : class, IEngineObject
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);

        FactoryMap<TKey> map = GetOrCreateMap<TKey>();

        map.Register(
            key,
            new Factory<TDefinition, TObject>(factory));
    }

    /// <summary>
    /// Registers a factory using the engine object type as its identifier.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// The type of definition accepted by the factory.
    /// </typeparam>
    /// <typeparam name="TObject">
    /// The type of engine object created by the factory.
    /// </typeparam>
    /// <param name="factory">
    /// The factory used to create the engine object.
    /// </param>
    public void Register<TDefinition, TObject>(
        Func<TDefinition, TObject> factory)
        where TDefinition : class, IEngineObjectDefinition
        where TObject : class, IEngineObject
    {
        ArgumentNullException.ThrowIfNull(factory);

        Register<Type, TDefinition, TObject>(
            typeof(TObject),
            factory);
    }

    /// <summary>
    /// Creates an engine object using the specified identifier and definition.
    /// </summary>
    /// <typeparam name="TKey">
    /// The identifier type.
    /// </typeparam>
    /// <param name="key">
    /// The registered identifier.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// No factory is registered for the specified identifier or the supplied
    /// definition is incompatible with the registered factory.
    /// </exception>
    public IEngineObject Create<TKey>(
        TKey key,
        IEngineObjectDefinition definition)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definition);

        IFactory factory = GetFactory(key);

        return factory.Create(definition);
    }

    /// <summary>
    /// Creates a strongly typed engine object using the specified identifier
    /// and definition.
    /// </summary>
    /// <typeparam name="TKey">
    /// The identifier type.
    /// </typeparam>
    /// <typeparam name="TObject">
    /// The expected engine object type.
    /// </typeparam>
    /// <param name="key">
    /// The registered identifier.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    public TObject Create<TKey, TObject>(
        TKey key,
        IEngineObjectDefinition definition)
        where TKey : notnull
        where TObject : class, IEngineObject
    {
        IEngineObject instance =
            Create(key, definition);

        if (instance is TObject result)
            return result;

        throw new InvalidOperationException(
            $"The factory registered for '{key}' created " +
            $"'{instance.GetType().FullName}', but " +
            $"'{typeof(TObject).FullName}' was requested.");
    }

    /// <summary>
    /// Creates an engine object using its registered engine object type.
    /// </summary>
    /// <param name="type">
    /// The registered engine object type.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    public IEngineObject Create(
        Type type,
        IEngineObjectDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(definition);

        return Create<Type>(
            type,
            definition);
    }

    /// <summary>
    /// Creates a strongly typed engine object using its registered type.
    /// </summary>
    /// <typeparam name="TObject">
    /// The registered engine object type.
    /// </typeparam>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    public TObject Create<TObject>(
        IEngineObjectDefinition definition)
        where TObject : class, IEngineObject
    {
        ArgumentNullException.ThrowIfNull(definition);

        return Create<Type, TObject>(
            typeof(TObject),
            definition);
    }

    /// <summary>
    /// Attempts to create an engine object using the specified identifier.
    /// </summary>
    /// <typeparam name="TKey">
    /// The identifier type.
    /// </typeparam>
    /// <param name="key">
    /// The registered identifier.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, contains the created
    /// engine object; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a compatible factory was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool TryCreate<TKey>(
        TKey key,
        IEngineObjectDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definition);

        instance = null;

        if (!TryGetFactory(
                key,
                out IFactory? factory))
        {
            return false;
        }

        if (!factory.CanCreate(definition))
            return false;

        instance = factory.Create(definition);

        return true;
    }

    /// <summary>
    /// Attempts to create a strongly typed engine object using the specified
    /// identifier.
    /// </summary>
    public bool TryCreate<TKey, TObject>(
        TKey key,
        IEngineObjectDefinition definition,
        [NotNullWhen(true)] out TObject? instance)
        where TKey : notnull
        where TObject : class, IEngineObject
    {
        instance = null;

        if (!TryCreate(
                key,
                definition,
                out IEngineObject? created))
        {
            return false;
        }

        if (created is not TObject result)
            return false;

        instance = result;

        return true;
    }

    /// <summary>
    /// Determines whether a factory is registered for the specified identifier.
    /// </summary>
    public bool IsRegistered<TKey>(
        TKey key)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(key);

        return TryGetFactory(
            key,
            out _);
    }

    /// <summary>
    /// Determines whether a factory is registered for the specified engine
    /// object type.
    /// </summary>
    public bool IsRegistered(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return IsRegistered<Type>(type);
    }

    /// <summary>
    /// Determines whether a factory is registered for the specified engine
    /// object type.
    /// </summary>
    public bool IsRegistered<TObject>()
        where TObject : class, IEngineObject
    {
        return IsRegistered<Type>(
            typeof(TObject));
    }

    /// <summary>
    /// Removes the factory registered for the specified identifier.
    /// </summary>
    public bool Unregister<TKey>(
        TKey key)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(key);

        Type keyType = typeof(TKey);

        if (!_factoryMaps.TryGetValue(
                keyType,
                out IFactoryMap? map))
        {
            return false;
        }

        FactoryMap<TKey> typedMap =
            (FactoryMap<TKey>)map;

        bool removed =
            typedMap.Remove(key);

        if (typedMap.Count == 0)
            _factoryMaps.Remove(keyType);

        return removed;
    }

    /// <summary>
    /// Removes the factory registered for the specified engine object type.
    /// </summary>
    public bool Unregister(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return Unregister<Type>(type);
    }

    /// <summary>
    /// Removes the factory registered for the specified engine object type.
    /// </summary>
    public bool Unregister<TObject>()
        where TObject : class, IEngineObject
    {
        return Unregister<Type>(
            typeof(TObject));
    }

    /// <summary>
    /// Removes all registered factories.
    /// </summary>
    public void Clear()
    {
        _factoryMaps.Clear();
    }

    private FactoryMap<TKey> GetOrCreateMap<TKey>()
        where TKey : notnull
    {
        Type keyType = typeof(TKey);

        if (_factoryMaps.TryGetValue(
                keyType,
                out IFactoryMap? map))
        {
            return (FactoryMap<TKey>)map;
        }

        var result =
            new FactoryMap<TKey>();

        _factoryMaps.Add(
            keyType,
            result);

        return result;
    }

    private IFactory GetFactory<TKey>(
        TKey key)
        where TKey : notnull
    {
        if (TryGetFactory(
                key,
                out IFactory? factory))
        {
            return factory;
        }

        throw new InvalidOperationException(
            $"No factory is registered for key '{key}' " +
            $"of type '{typeof(TKey).FullName}'.");
    }

    private bool TryGetFactory<TKey>(
        TKey key,
        [NotNullWhen(true)] out IFactory? factory)
        where TKey : notnull
    {
        factory = null;

        if (!_factoryMaps.TryGetValue(
                typeof(TKey),
                out IFactoryMap? map))
        {
            return false;
        }

        FactoryMap<TKey> typedMap =
            (FactoryMap<TKey>)map;

        return typedMap.TryGet(
            key,
            out factory);
    }

    private interface IFactoryMap
    {
        int Count { get; }
    }

    private sealed class FactoryMap<TKey> :
        IFactoryMap
        where TKey : notnull
    {
        private readonly Dictionary<TKey, IFactory> _factories = new();

        public int Count =>
            _factories.Count;

        public void Register(
            TKey key,
            IFactory factory)
        {
            _factories[key] = factory;
        }

        public bool TryGet(
            TKey key,
            [NotNullWhen(true)] out IFactory? factory)
        {
            return _factories.TryGetValue(
                key,
                out factory);
        }

        public bool Remove(TKey key)
        {
            return _factories.Remove(key);
        }
    }

    private interface IFactory
    {
        Type DefinitionType { get; }

        Type ObjectType { get; }

        bool CanCreate(
            IEngineObjectDefinition definition);

        IEngineObject Create(
            IEngineObjectDefinition definition);
    }

    private sealed class Factory<TDefinition, TObject> :
        IFactory
        where TDefinition : class, IEngineObjectDefinition
        where TObject : class, IEngineObject
    {
        private readonly Func<TDefinition, TObject> _factory;

        public Factory(
            Func<TDefinition, TObject> factory)
        {
            ArgumentNullException.ThrowIfNull(factory);

            _factory = factory;
        }

        public Type DefinitionType =>
            typeof(TDefinition);

        public Type ObjectType =>
            typeof(TObject);

        public bool CanCreate(
            IEngineObjectDefinition definition)
        {
            return definition is TDefinition;
        }

        public IEngineObject Create(
            IEngineObjectDefinition definition)
        {
            if (definition is not TDefinition typedDefinition)
            {
                throw new InvalidOperationException(
                    $"Factory for '{typeof(TObject).FullName}' requires " +
                    $"definition '{typeof(TDefinition).FullName}', but " +
                    $"'{definition.GetType().FullName}' was provided.");
            }

            TObject instance =
                _factory(typedDefinition);

            if (instance is null)
            {
                throw new InvalidOperationException(
                    $"Factory for '{typeof(TObject).FullName}' returned null.");
            }

            return instance;
        }
    }
}