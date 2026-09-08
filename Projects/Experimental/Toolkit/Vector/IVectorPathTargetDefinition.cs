using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using Sachssoft.Sasogine.Experimental.Components.Tools.Vector;

namespace Sachssoft.Sasogine.Experimental.Components.Tools;

/// <summary>
/// Defines a selection target definition that can be edited by a vector path tool.
/// </summary>
public interface IVectorPathTargetDefinition : IDefinition, ISelectionTargetDefinition
{
    /// <summary>
    /// Gets or sets the active vector shape to edit,
    /// or <see langword="null"/> if no vector shape is active.
    /// </summary>
    VectorShape? ActiveShape { get; set; }
}