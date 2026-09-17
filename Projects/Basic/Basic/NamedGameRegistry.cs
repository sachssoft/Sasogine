using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides a game registry that uses named registry keys.
/// </summary>
/// <typeparam name="TDefinition">The base type of registered definitions.</typeparam>
/// <typeparam name="TObject">The base type of registered engine objects.</typeparam>
public class NamedGameRegistry<TDefinition, TObject> :
    GameRegistry<NamedGameRegistryKey, TDefinition, TObject>
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="NamedGameRegistry{TDefinition, TObject}"/> class.
    /// </summary>
    public NamedGameRegistry()
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="NamedGameRegistry{TDefinition, TObject}"/> class.
    /// </summary>
    /// <param name="definitionMatchMode">The definition matching mode.</param>
    public NamedGameRegistry(DefinitionMatchMode definitionMatchMode)
        : base(definitionMatchMode)
    {
    }

    /// <summary>
    /// Registers the specified definition and engine object factories.
    /// </summary>
    /// <param name="name">The registry name.</param>
    /// <param name="definitionFactory">The factory used to create definitions.</param>
    /// <param name="objectFactory">The factory used to create engine objects.</param>
    public void Register(
        string name,
        Func<TDefinition> definitionFactory,
        Func<TDefinition, TObject> objectFactory)
    {
        Register(
            new NamedGameRegistryKey(name),
            definitionFactory,
            objectFactory);
    }

    /// <summary>
    /// Creates a definition registered with the specified name.
    /// </summary>
    /// <param name="name">The registry name.</param>
    /// <returns>The created definition.</returns>
    public TDefinition CreateDefinition(string name)
    {
        return CreateDefinition(new NamedGameRegistryKey(name));
    }

    /// <summary>
    /// Attempts to create a definition registered with the specified name.
    /// </summary>
    /// <param name="name">The registry name.</param>
    /// <param name="definition">The created definition.</param>
    /// <returns>
    /// <see langword="true"/> if the definition was created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryCreateDefinition(string name, out TDefinition? definition)
    {
        return TryCreateDefinition(
            new NamedGameRegistryKey(name),
            out definition);
    }
}