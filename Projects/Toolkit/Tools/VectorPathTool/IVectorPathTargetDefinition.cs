using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using Sachssoft.Sasogine.Components.Tools.Vector;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Defines a selection target definition that can be edited by a vector path tool.
/// </summary>
public interface IVectorPathTargetDefinition : IDefinition, ISelectionTargetDefinition
{
    /// <summary>
    /// Gets the active vector shape definition to edit,
    /// or <see langword="null"/> if no vector shape is active.
    /// </summary>
    [Browsable(false)]
    VectorShapeDefinition? ActiveShape { get; }
}