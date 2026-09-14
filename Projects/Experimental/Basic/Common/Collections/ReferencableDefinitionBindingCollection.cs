using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Common.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Common.Collections;

/// <summary>
/// Provides a read-only definition binding collection with engine object
/// reference resolution.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition used to create engine objects.
/// </typeparam>
/// <typeparam name="TObject">
/// The type of referenceable engine object.
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
    public ReferencableDefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject> factory)
        : this(
            definitions,
            factory,
            new ReferencableCollection<TObject>(definitions.Count))
    {
    }

    private ReferencableDefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject> factory,
        ReferencableCollection<TObject> objects)
        : base(definitions, factory)
    {
        _objects = objects;
    }

    /// <summary>
    /// Finds an engine object with the specified identifier.
    /// </summary>
    public TObject? Find(string? id) =>
        _objects.Find(id);

    /// <summary>
    /// Attempts to find an engine object with the specified identifier.
    /// </summary>
    public bool TryGet(
        string? id,
        [MaybeNullWhen(false)] out TObject result) =>
        _objects.TryGet(id, out result);

    IEngineReferenceable? IEngineObjectResolver.Find(string? id) =>
        _objects.Find(id);

    bool IEngineObjectResolver.TryGet(
        string? id,
        [MaybeNullWhen(false)] out IEngineReferenceable? result)
    {
        result = _objects.Find(id);
        return result is not null;
    }

    IEnumerable<IEngineReferenceable>
        IEngineObjectResolver.FindAll(string? @class) =>
        ((IEngineObjectResolver)_objects).FindAll(@class);
}