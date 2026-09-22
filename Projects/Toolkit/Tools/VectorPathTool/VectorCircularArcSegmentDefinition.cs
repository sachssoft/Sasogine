using Sachssoft.Engine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines the configurable state of a circular arc vector segment.
/// </summary>
public class VectorCircularArcSegmentDefinition : VectorSegmentDefinition
{
    /// <summary>
    /// Gets the control node used to define the geometry of the circular arc.
    /// </summary>
    /// <value>
    /// The control node that influences the shape and curvature of the arc.
    /// </value>
    [Category(Categories.Design)]
    [DisplayName("Control Node")]
    public VectorNodeDefinition ControlNode { get; } = new();
}