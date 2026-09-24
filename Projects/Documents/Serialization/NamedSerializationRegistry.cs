using Sachssoft.Engine;
using System;

namespace Sachssoft.Documents.Serialization;

/// <summary>
/// Provides a serialization registry that uses named registry keys.
/// </summary>
/// <typeparam name="TDefinition">The base type of registered definitions.</typeparam>
/// <typeparam name="TObject">The base type of registered engine objects.</typeparam>
public class NamedSerializationRegistry<TDefinition, TObject> :
    SerializationRegistry<NamedGameRegistryKey, TDefinition, TObject>
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="NamedSerializationRegistry{TDefinition, TObject}"/> class.
    /// </summary>
    public NamedSerializationRegistry()
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="NamedSerializationRegistry{TDefinition, TObject}"/> class.
    /// </summary>
    /// <param name="definitionMatchMode">The definition matching mode.</param>
    public NamedSerializationRegistry(DefinitionMatchMode definitionMatchMode)
        : base(definitionMatchMode)
    {
    }

    /// <summary>
    /// Registers the specified serialization, definition and object factories.
    /// </summary>
    /// <param name="name">The registry name.</param>
    /// <param name="serialization">The serialization handler.</param>
    /// <param name="definitionFactory">The factory used to create definitions.</param>
    /// <param name="objectFactory">The factory used to create engine objects.</param>
    public void Register(
        string name,
        ISerialization serialization,
        Func<TDefinition> definitionFactory,
        Func<TDefinition, TObject> objectFactory)
    {
        Register(
            new NamedGameRegistryKey(name),
            serialization,
            definitionFactory,
            objectFactory);
    }
}