using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Common.Collections;

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
    public static IBindingCollection<TObject> Connect<TDefinition, TObject>(
        IEngineObject connectionOwner,
        DefinitionBindingCollection<TDefinition, TObject> collection)
        where TDefinition : class, IDefinition
        where TObject : class, IEngineObject
    {
        ArgumentNullException.ThrowIfNull(connectionOwner);
        ArgumentNullException.ThrowIfNull(collection);

        return collection.Connect(connectionOwner);
    }

    /// <summary>
    /// Creates a context-aware definition binding collection for the specified
    /// definitions, factory, and runtime context.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// The type of definition contained in the source collection.
    /// </typeparam>
    /// <typeparam name="TObject">
    /// The type of engine object created from the definitions.
    /// </typeparam>
    /// <typeparam name="TContext">
    /// The type of runtime context used when creating and managing engine objects.
    /// </typeparam>
    /// <param name="definitions">
    /// The source collection of definitions to bind.
    /// </param>
    /// <param name="factory">
    /// The factory used to create, attach, and release engine objects.
    /// </param>
    /// <param name="context">
    /// The runtime context used by the binding factory.
    /// </param>
    /// <returns>
    /// A new context-aware definition binding collection synchronized with the
    /// specified definition collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="definitions"/>, <paramref name="factory"/>, or
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public static DefinitionBindingCollection<TDefinition, TObject, TContext>
        Create<TDefinition, TObject, TContext>(
            TrackableCollection<TDefinition> definitions,
            IDefinitionBindingFactory<TDefinition, TObject, TContext> factory,
            TContext context)
        where TDefinition : class, IDefinition
        where TObject : class, IEngineObject
        where TContext : class
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(context);

        return new DefinitionBindingCollection<TDefinition, TObject, TContext>(
            definitions,
            factory,
            context);
    }

    /// <summary>
    /// Creates a referenceable definition binding collection associated with the
    /// specified connection owner.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// The type of definition used to create engine objects.
    /// </typeparam>
    /// <typeparam name="TObject">
    /// The type of referenceable engine object exposed by the collection.
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
    /// The created referenceable definition binding collection.
    /// </returns>
    public static ReferencableDefinitionBindingCollection<TDefinition, TObject>
        CreateReferencable<TDefinition, TObject>(
            IEngineObject connectionOwner,
            TrackableCollection<TDefinition> definitions,
            IDefinitionBindingFactory<TDefinition, TObject> factory)
        where TDefinition : class, IDefinition
        where TObject : class, IEngineObject, IEngineReferenceable
    {
        ArgumentNullException.ThrowIfNull(connectionOwner);
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(factory);

        return new ReferencableDefinitionBindingCollection<TDefinition, TObject>(
            definitions,
            factory,
            connectionOwner);
    }
}