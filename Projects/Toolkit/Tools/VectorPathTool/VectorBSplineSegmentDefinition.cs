using Sachssoft.Engine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines the configurable state of a B-spline segment.
/// </summary>
public sealed class VectorBSplineSegmentDefinition : VectorVariableSegmentDefinition
{
    /// <summary>
    /// Gets or sets the polynomial degree of the B-spline curve.
    /// </summary>
    /// <value>
    /// The polynomial degree used to construct the B-spline curve.
    /// A degree of <c>1</c> produces a linear B-spline, <c>2</c> a quadratic
    /// B-spline, and <c>3</c> a cubic B-spline. Higher values produce
    /// higher-degree spline curves.
    /// The default value is <c>3</c>.
    /// </value>
    /// <remarks>
    /// The degree determines the polynomial order and the number of control
    /// points that can influence each section of the curve. A B-spline of
    /// degree <c>p</c> requires at least <c>p + 1</c> control points.
    /// </remarks>
    [Category(Categories.Design)]
    [DisplayName("Degree")]
    public int Degree { get; set; } = 3;
}