using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Components.Tools.Vector;

/// <summary>
/// Defines the configurable state shared by vector segments that support
/// a variable number of control nodes.
/// </summary>
public abstract class VectorVariableSegmentDefinition : VectorSegmentDefinition
{
    /// <summary>
    /// Gets the collection of control node definitions used to define
    /// or influence the geometry of the vector segment.
    /// </summary>
    /// <remarks>
    /// The collection is intended to be managed through specialized vector
    /// editing tools rather than directly through the property inspector.
    /// </remarks>
    [Browsable(false)]
    public ObservableCollection<VectorNodeDefinition> ControlNodes { get; } = [];
}