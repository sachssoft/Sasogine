using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common.Collections;

/// <summary>
/// Provides controlled mutable connections to definition binding collections.
/// </summary>
/// <remarks>
/// A binding collection created through this type can be associated with a
/// specific engine object. Only that engine object can establish the mutable
/// connection to the collection.
/// </remarks>
public static class DefinitionBindingConnection
{
    /// <summary>
    /// Creates a definition binding collection associated with the specified
    /// connection owner.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// The type of definition used to create engine objects.
    /// </typeparam>
    /// <typeparam name="TObject">
    /// The type of engine object exposed by the collection.
    /// </typeparam>
    /// <param name="connectionOwner">
    /// The engine object that is allowed to establish the mutable connection.
    /// </param>
    /// <param name="definitions">
    /// The definition collection to bind.
    /// </param>
    /// <param name="factory">
    /// The factory used to create and release engine objects.
    /// </param>
    /// <returns>
    /// The created definition binding collection.
    /// </returns>
    public static DefinitionBindingCollection<TDefinition, TObject> Create<TDefinition, TObject>(
        IEngineObject connectionOwner,
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject> factory)
        where TDefinition : class, IDefinition
        where TObject : class, IEngineObject
    {
        ArgumentNullException.ThrowIfNull(connectionOwner);
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(factory);

        return new DefinitionBindingCollection<TDefinition, TObject>(
            definitions,
            factory,
            connectionOwner);
    }

    /// <summary>
    /// Establishes the mutable connection to the specified binding collection.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// The type of definition used to create engine objects.
    /// </typeparam>
    /// <typeparam name="TObject">
    /// The type of engine object exposed by the collection.
    /// </typeparam>
    /// <param name="connectionOwner">
    /// The engine object requesting mutable access.
    /// </param>
    /// <param name="collection">
    /// The binding collection to connect to.
    /// </param>
    /// <returns>
    /// A mutable list connected to the underlying definition collection.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The requesting engine object is not the registered connection owner,
    /// or the mutable connection has already been established.
    /// </exception>
    public static IList<TObject> Connect<TDefinition, TObject>(
        IEngineObject connectionOwner,
        DefinitionBindingCollection<TDefinition, TObject> collection)
        where TDefinition : class, IDefinition
        where TObject : class, IEngineObject
    {
        ArgumentNullException.ThrowIfNull(connectionOwner);
        ArgumentNullException.ThrowIfNull(collection);

        return collection.Connect(connectionOwner);
    }
}