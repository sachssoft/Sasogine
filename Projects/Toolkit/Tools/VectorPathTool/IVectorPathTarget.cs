using Sachssoft.Sasogine.Components.Tools.Selection;
using Sachssoft.Sasogine.Components.Tools.Vector;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Defines a selection target that provides a vector shape for editing
/// by a vector path tool.
/// </summary>
public interface IVectorPathTarget : ISelectionTarget
{
    /// <summary>
    /// Gets the vector path target definition associated with this target.
    /// </summary>
    new IVectorPathTargetDefinition Definition { get; }

    /// <summary>
    /// Gets the active vector shape to edit.
    /// </summary>
    /// <value>
    /// The active vector shape, or <see langword="null"/> if no shape is active.
    /// </value>
    VectorShape? ActiveShape { get; }
}