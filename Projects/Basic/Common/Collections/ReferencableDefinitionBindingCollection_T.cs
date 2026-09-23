using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine.Common.Collections;

/// <summary>
/// Provides a context-aware read-only definition binding collection with
/// engine object reference resolution.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition used to create engine objects.
/// </typeparam>
/// <typeparam name="TObject">
/// The type of referenceable engine object exposed by the collection.
/// </typeparam>
/// <typeparam name="TContext">
/// The type of runtime context used to manage engine objects.
/// </typeparam>
public class ReferencableDefinitionBindingCollection<
    TDefinition,
    TObject,
    TContext> :
    DefinitionBindingCollection<TDefinition, TObject, TContext>,
    IEngineObjectResolver
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject, IEngineReferenceable
    where TContext : class
{
    private readonly ReferencableCollection<TObject> _objects;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReferencableDefinitionBindingCollection{TDefinition, TObject, TContext}"/>
    /// class.
    /// </summary>
    /// <param name="definitions">
    /// The definition collection to observe.
    /// </param>
    /// <param name="factory">
    /// The factory used to create and release engine objects.
    /// </param>
    /// <param name="context">
    /// The optional runtime context.
    /// </param>
    public ReferencableDefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject, TContext> factory,
        TContext? context = null)
        : this(
            definitions,
            factory,
            new ReferencableCollection<TObject>(definitions.Count),
            context)
    {
    }

    private ReferencableDefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject, TContext> factory,
        ReferencableCollection<TObject> objects,
        TContext? context)
        : base(
            definitions,
            factory,
            objects,
            context)
    {
        _objects = objects;
    }

    /// <summary>
    /// Finds an engine object with the specified identifier.
    /// </summary>
    public TObject? Find(string? id) =>
        _objects.Find(id);

    /// <summary>
    /// Attempts to get an engine object with the specified identifier.
    /// </summary>
    public bool TryGet(
        string? id,
        [MaybeNullWhen(false)] out TObject result) =>
        _objects.TryGet(id, out result);

    /// <summary>
    /// Finds all engine objects with the specified class.
    /// </summary>
    public IEnumerable<IEngineReferenceable> FindAll(string? @class) =>
        ((IEngineObjectResolver)_objects).FindAll(@class);

    IEngineReferenceable? IEngineObjectResolver.Find(string? id) =>
        ((IEngineObjectResolver)_objects).Find(id);

    bool IEngineObjectResolver.TryGet(
        string? id,
        [MaybeNullWhen(false)] out IEngineReferenceable? result) =>
        ((IEngineObjectResolver)_objects).TryGet(
            id,
            out result);

    IEnumerable<IEngineReferenceable>
        IEngineObjectResolver.FindAll(string? @class) =>
        ((IEngineObjectResolver)_objects).FindAll(@class);
}