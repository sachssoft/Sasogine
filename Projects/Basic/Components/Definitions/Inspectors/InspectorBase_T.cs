using System;

namespace Sachssoft.Engine.Components.Definitions.Inspectors;

/// <summary>
/// Provides strongly typed access to an inspector's definition.
/// </summary>
/// <typeparam name="TDefinition">The described definition type.</typeparam>
public abstract class InspectorBase<TDefinition> : IInspector
    where TDefinition : class
{
    /// <summary>
    /// Builds the property description for the supplied definition.
    /// </summary>
    public abstract void Build(
        IInspectorView view,
        TDefinition definition);

    /// <inheritdoc/>
    void IInspector.Build(IInspectorView view, object source)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(source);

        if (source is not TDefinition definition)
        {
            throw new ArgumentException(
                "The source is not compatible with this inspector.",
                nameof(source));
        }

        Build(view, definition);
    }
}