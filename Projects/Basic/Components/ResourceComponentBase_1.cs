using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components;

/// <summary>
/// Provides a base implementation for definition-based components
/// that manage loadable resources.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of component definition.
/// </typeparam>
public abstract class ResourceComponentBase<TDefinition> :
    EngineObject<TDefinition>,
    IResourceComponent
    where TDefinition : class, IComponentDefinition
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ResourceComponentBase{TDefinition}"/> class.
    /// </summary>
    /// <param name="definition">
    /// The definition associated with the component.
    /// </param>
    protected ResourceComponentBase(TDefinition definition)
        : base(definition)
    {
    }
}