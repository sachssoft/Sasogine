using System;

namespace Sachssoft.Engine.Controls;

/// <summary>Adapts an explicitly typed factory to the template contract.</summary>
/// <typeparam name="TSource">The factory input type.</typeparam>
/// <typeparam name="TResult">The presentation type.</typeparam>
public sealed class DelegateTemplate<TSource, TResult> : ITemplate<TSource, TResult>
{
    /// <summary>Stores the factory invoked for each presentation.</summary>
    private readonly Func<TSource, TResult> _factory;

    /// <summary>Creates a template using the supplied factory.</summary>
    /// <param name="factory">The factory; it must create a fresh presentation on each call.</param>
    public DelegateTemplate(Func<TSource, TResult> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    /// <inheritdoc/>
    public TResult Create(TSource source) => _factory(source);
}
