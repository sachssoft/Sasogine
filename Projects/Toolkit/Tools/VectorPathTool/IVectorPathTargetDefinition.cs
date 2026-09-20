using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using Sachssoft.Sasogine.Components.Tools.Vector;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Components.Tools;

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