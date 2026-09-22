using System;

namespace Sachssoft.Engine.Common.Collections;

/// <summary>
/// Provides helpers for creating context-aware definition binding collections.
/// </summary>
public static class DefinitionBindingConnectionContextExtensions
{
    /// <summary>
    /// Creates a context-aware definition binding collection for the specified
    /// definitions, factory, and optional runtime context.
    /// </summary>
    /// <typeparam name="TDefinition">The type of definition contained in the source collection.</typeparam>
    /// <typeparam name="TObject">The type of engine object created from the definitions.</typeparam>
    /// <typeparam name="TContext">The type of runtime context used by the binding factory.</typeparam>
    /// <param name="definitions">The source collection of definitions to bind.</param>
    /// <param name="factory">The factory used to create, attach, and release engine objects.</param>
    /// <param name="context">The optional runtime context used by the binding factory.</param>
    /// <returns>A new context-aware definition binding collection.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="definitions"/> or <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    public static DefinitionBindingCollection<TDefinition, TObject, TContext>
        Create<TDefinition, TObject, TContext>(
            TrackableCollection<TDefinition> definitions,
            IDefinitionBindingFactory<TDefinition, TObject, TContext> factory,
            TContext? context = null)
        where TDefinition : class, IDefinition
        where TObject : class, IEngineObject
        where TContext : class
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(factory);

        return new DefinitionBindingCollection<TDefinition, TObject, TContext>(
            definitions,
            factory,
            context);
    }
}
