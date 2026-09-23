using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine.Common.Collections;

/// <summary>
/// Provides a read-only definition binding collection with engine object
/// reference resolution.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition used to create engine objects.
/// </typeparam>
/// <typeparam name="TObject">
/// The type of referenceable engine object exposed by the collection.
/// </typeparam>
public class ReferencableDefinitionBindingCollection<TDefinition, TObject> :
    DefinitionBindingCollection<TDefinition, TObject>,
    IEngineObjectResolver
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject, IEngineReferenceable
{
    private readonly ReferencableCollection<TObject> _objects;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReferencableDefinitionBindingCollection{TDefinition, TObject}"/>
    /// class.
    /// </summary>
    /// <param name="definitions">
    /// The definition collection to observe.
    /// </param>
    /// <param name="factory">
    /// The factory used to create and release engine objects.
    /// </param>
    public ReferencableDefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject> factory)
        : this(
            definitions,
            factory,
            new ReferencableCollection<TObject>(definitions.Count),
            null)
    {
    }

    internal ReferencableDefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject> factory,
        IEngineObject connectionOwner)
        : this(
            definitions,
            factory,
            new ReferencableCollection<TObject>(definitions.Count),
            connectionOwner)
    {
    }

    private ReferencableDefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject> factory,
        ReferencableCollection<TObject> objects,
        IEngineObject? connectionOwner)
        : base(
            definitions,
            factory,
            objects,
            connectionOwner)
    {
        _objects = objects;
    }

    /// <summary>
    /// Finds an engine object with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the engine object to find.
    /// </param>
    /// <returns>
    /// The matching engine object, or <see langword="null"/> if no matching
    /// object exists.
    /// </returns>
    public TObject? Find(string? id) =>
        _objects.Find(id);

    /// <summary>
    /// Attempts to get an engine object with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the engine object to find.
    /// </param>
    /// <param name="result">
    /// The matching engine object if found; otherwise,
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching engine object was found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet(
        string? id,
        [MaybeNullWhen(false)] out TObject result) =>
        _objects.TryGet(id, out result);

    /// <summary>
    /// Finds all engine objects with the specified class.
    /// </summary>
    /// <param name="class">
    /// The engine object class to find.
    /// </param>
    /// <returns>
    /// The matching engine objects.
    /// </returns>
    public IEnumerable<IEngineReferenceable> FindAll(string? @class) =>
        ((IEngineObjectResolver)_objects).FindAll(@class);

    IEngineReferenceable? IEngineObjectResolver.Find(string? id) =>
        ((IEngineObjectResolver)_objects).Find(id);

    bool IEngineObjectResolver.TryGet(
        string? id,
        [MaybeNullWhen(false)] out IEngineReferenceable? result) =>
        ((IEngineObjectResolver)_objects).TryGet(id, out result);

    IEnumerable<IEngineReferenceable>
        IEngineObjectResolver.FindAll(string? @class) =>
        ((IEngineObjectResolver)_objects).FindAll(@class);
}