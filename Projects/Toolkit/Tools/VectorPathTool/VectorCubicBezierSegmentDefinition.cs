using Sachssoft.Sasogine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Defines the configurable state of a cubic Bézier vector segment.
/// </summary>
public class VectorCubicBezierSegmentDefinition : VectorSegmentDefinition
{
    /// <summary>
    /// Gets the first control node used to define the shape of the cubic Bézier curve.
    /// </summary>
    /// <value>
    /// The first control node influencing the curve from its starting point.
    /// </value>
    [Category(Categories.Design)]
    [DisplayName("Control Node 0")]
    public VectorNodeDefinition ControlNode0 { get; } = new();

    /// <summary>
    /// Gets the second control node used to define the shape of the cubic Bézier curve.
    /// </summary>
    /// <value>
    /// The second control node influencing the curve toward its end point.
    /// </value>
    [Category(Categories.Design)]
    [DisplayName("Control Node 1")]
    public VectorNodeDefinition ControlNode1 { get; } = new();
}