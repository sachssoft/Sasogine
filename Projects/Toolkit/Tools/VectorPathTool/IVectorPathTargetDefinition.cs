using System.ComponentModel;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines a selection target definition that can be edited by a vector path tool.
/// </summary>
public interface IVectorPathTargetDefinition :
    IDefinition,
    ISelectionTargetDefinition
{
    /// <summary>
    /// Gets the initial vector shape definition used to create the active shape.
    /// </summary>
    [Browsable(false)]
    VectorShapeDefinition Shape { get; }
}