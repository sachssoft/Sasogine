using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Provides a specialized game registry that uses an enumeration value
/// together with a string identifier as its registry key.
/// </summary>
/// <typeparam name="TEnum">The enumeration type used as the registry key value.</typeparam>
/// <typeparam name="TDefinition">The base type of definitions managed by the registry.</typeparam>
/// <typeparam name="TObject">The base type of engine objects managed by the registry.</typeparam>
public class IndexedGameRegistry<TEnum, TDefinition, TObject>
    : GameRegistry<IndexedGameRegistryKey<TEnum>, TDefinition, TObject>
    where TEnum : struct, Enum
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexedGameRegistry{TEnum, TDefinition, TObject}"/>
    /// class using exact definition type matching.
    /// </summary>
    public IndexedGameRegistry()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexedGameRegistry{TEnum, TDefinition, TObject}"/> class.
    /// </summary>
    /// <param name="definitionMatchMode">The mode used to match definition types.</param>
    public IndexedGameRegistry(DefinitionMatchMode definitionMatchMode)
        : base(definitionMatchMode)
    {
    }

    /// <summary>
    /// Registers definition and engine object factories using the specified name and index.
    /// </summary>
    /// <param name="name">The string identifier of the registry key.</param>
    /// <param name="index">The enumeration value of the registry key.</param>
    /// <param name="definitionFactory">The factory used to create definitions.</param>
    /// <param name="objectFactory">The factory used to create engine objects.</param>
    public void Register(
        string name,
        TEnum index,
        Func<TDefinition> definitionFactory,
        Func<TDefinition, TObject> objectFactory)
    {
        Register(
            new IndexedGameRegistryKey<TEnum>(name, index),
            definitionFactory,
            objectFactory);
    }

    /// <summary>
    /// Creates a definition using the specified name and index.
    /// </summary>
    /// <param name="name">The string identifier of the registry key.</param>
    /// <param name="index">The enumeration value of the registry key.</param>
    /// <returns>The created definition.</returns>
    public TDefinition CreateDefinition(string name, TEnum index)
    {
        return CreateDefinition(
            new IndexedGameRegistryKey<TEnum>(name, index));
    }

    /// <summary>
    /// Attempts to create a definition using the specified name and index.
    /// </summary>
    /// <param name="name">The string identifier of the registry key.</param>
    /// <param name="index">The enumeration value of the registry key.</param>
    /// <param name="definition">The created definition, if successful.</param>
    /// <returns>
    /// <see langword="true"/> if the definition could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryCreateDefinition(
        string name,
        TEnum index,
        out TDefinition? definition)
    {
        return TryCreateDefinition(
            new IndexedGameRegistryKey<TEnum>(name, index),
            out definition);
    }
}