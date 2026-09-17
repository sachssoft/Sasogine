using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Provides a specialized game registry that uses an enumeration value
/// together with a string identifier as its registry key.
/// </summary>
/// <typeparam name="TEnum">
/// The enumeration type used to index registered engine object factories.
/// </typeparam>
public class IndexedGameRegistry<TEnum> : GameRegistry
    where TEnum : struct, Enum
{
    /// <summary>
    /// Registers an engine object factory using the specified name and index.
    /// </summary>
    public void Register<TDefinition, TObject>(
        string name,
        TEnum index,
        Func<TDefinition, TObject> factory)
        where TDefinition : class, IEngineObjectDefinition
        where TObject : class, IEngineObject
    {
        Register<GameRegistryKey<TEnum>, TDefinition, TObject>(
            new GameRegistryKey<TEnum>(name, index),
            factory);
    }

    /// <summary>
    /// Creates an engine object using the specified name, index,
    /// and definition.
    /// </summary>
    public IEngineObject Create(
        string name,
        TEnum index,
        IEngineObjectDefinition definition)
    {
        return Create(
            new GameRegistryKey<TEnum>(name, index),
            definition);
    }

    /// <summary>
    /// Creates a strongly typed engine object using the specified name,
    /// index, and definition.
    /// </summary>
    public TObject Create<TObject>(
        string name,
        TEnum index,
        IEngineObjectDefinition definition)
        where TObject : class, IEngineObject
    {
        return Create<GameRegistryKey<TEnum>, TObject>(
            new GameRegistryKey<TEnum>(name, index),
            definition);
    }

    /// <summary>
    /// Attempts to create an engine object using the specified name,
    /// index, and definition.
    /// </summary>
    public bool TryCreate(
        string name,
        TEnum index,
        IEngineObjectDefinition definition,
        out IEngineObject? instance)
    {
        return TryCreate(
            new GameRegistryKey<TEnum>(name, index),
            definition,
            out instance);
    }

    /// <summary>
    /// Determines whether a factory is registered for the specified
    /// name and index.
    /// </summary>
    public bool IsRegistered(
        string name,
        TEnum index)
    {
        return IsRegistered(
            new GameRegistryKey<TEnum>(name, index));
    }

    /// <summary>
    /// Removes the factory registered for the specified name and index.
    /// </summary>
    public bool Unregister(
        string name,
        TEnum index)
    {
        return Unregister(
            new GameRegistryKey<TEnum>(name, index));
    }
}