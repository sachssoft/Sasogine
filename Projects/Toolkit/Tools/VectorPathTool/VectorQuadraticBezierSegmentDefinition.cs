using Sachssoft.Engine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines a quadratic Bézier vector segment using a single control node.
/// </summary>
public class VectorQuadraticBezierSegmentDefinition : VectorSegmentDefinition
{
    /// <summary>
    /// Gets the control node used to define the curvature of the quadratic Bézier segment.
    /// </summary>        
    [Category(Categories.Design)]
    [DisplayName("Control Node")]
    public VectorNodeDefinition ControlNode { get; } = new();
}