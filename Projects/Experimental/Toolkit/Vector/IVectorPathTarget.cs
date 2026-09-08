using Sachssoft.Sasogine.Components.Tools.Selection;
using Sachssoft.Sasogine.Experimental.Components.Tools.Vector;

namespace Sachssoft.Sasogine.Experimental.Components.Tools;

/// <summary>
/// Defines a selection target that can be edited by a vector path tool.
/// </summary>
public interface IVectorPathTarget : ISelectionTarget
{
    /// <summary>
    /// Gets or sets the active vector shape to edit,
    /// or <see langword="null"/> if no vector shape is active.
    /// </summary>
    VectorShape? ActiveShape { get; set; }
}