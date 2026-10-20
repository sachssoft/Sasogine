using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine.Components.Definitions.Inspectors;

/// <summary>
/// Maps exact definition types to explicitly registered inspector factories.
/// </summary>
public sealed class InspectorRegistry
{
    private readonly Dictionary<Type, Func<IInspector>> _factories = new();

    /// <summary>
    /// Gets the number of registered definition types.
    /// </summary>
    public int Count => _factories.Count;

    /// <summary>
    /// Registers or replaces the inspector factory for a definition type.
    /// </summary>
    public void Register<TDefinition>(
        Func<InspectorBase<TDefinition>> factory)
        where TDefinition : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factories[typeof(TDefinition)] = () =>
            factory() ?? throw new InvalidOperationException(
                "The inspector factory returned null.");
    }

    /// <summary>
    /// Determines whether the exact definition type is registered.
    /// </summary>
    public bool Contains<TDefinition>()
        where TDefinition : class
    {
        return _factories.ContainsKey(typeof(TDefinition));
    }

    /// <summary>
    /// Creates the inspector for the source's exact runtime type.
    /// </summary>
    public bool TryCreate(
        object source,
        [NotNullWhen(true)] out IInspector? inspector)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (_factories.TryGetValue(source.GetType(), out var factory))
        {
            inspector = factory();
            return true;
        }

        inspector = null;
        return false;
    }

    /// <summary>
    /// Builds the registered property description in the supplied view.
    /// </summary>
    public void Build(IInspectorView view, object source)
    {
        ArgumentNullException.ThrowIfNull(view);

        if (!TryCreate(source, out var inspector))
        {
            throw new NotSupportedException(
                "No inspector is registered for the supplied definition type.");
        }

        inspector.Build(view, source);
    }
}