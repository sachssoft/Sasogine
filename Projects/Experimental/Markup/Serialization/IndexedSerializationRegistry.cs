using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Experimental;
using System;

namespace Sachssoft.Sasogine.Markup.Serialization;

/// <summary>
/// Provides a serialization registry that uses indexed registry keys.
/// </summary>
/// <typeparam name="TEnum">The enumeration type used as the index value.</typeparam>
/// <typeparam name="TDefinition">The base type of registered definitions.</typeparam>
/// <typeparam name="TObject">The base type of registered engine objects.</typeparam>
public class IndexedSerializationRegistry<TEnum, TDefinition, TObject> :
    SerializationRegistry<IndexedGameRegistryKey<TEnum>, TDefinition, TObject>
    where TEnum : struct, Enum
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="IndexedSerializationRegistry{TEnum, TDefinition, TObject}"/> class.
    /// </summary>
    public IndexedSerializationRegistry()
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="IndexedSerializationRegistry{TEnum, TDefinition, TObject}"/> class.
    /// </summary>
    /// <param name="definitionMatchMode">The definition matching mode.</param>
    public IndexedSerializationRegistry(DefinitionMatchMode definitionMatchMode)
        : base(definitionMatchMode)
    {
    }

    /// <summary>
    /// Registers the specified serialization, definition and object factories.
    /// </summary>
    /// <param name="name">The registry name.</param>
    /// <param name="index">The registry index.</param>
    /// <param name="serialization">The serialization handler.</param>
    /// <param name="definitionFactory">The factory used to create definitions.</param>
    /// <param name="objectFactory">The factory used to create engine objects.</param>
    public void Register(
        string name,
        TEnum index,
        ISerialization serialization,
        Func<TDefinition> definitionFactory,
        Func<TDefinition, TObject> objectFactory)
    {
        Register(
            new IndexedGameRegistryKey<TEnum>(name, index),
            serialization,
            definitionFactory,
            objectFactory);
    }
}