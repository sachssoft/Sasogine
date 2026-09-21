using Sachssoft.Sasogine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Defines the configurable state of a Catmull-Rom segment.
/// </summary>
public sealed class VectorCatmullRomSegmentDefinition : VectorVariableSegmentDefinition
{
    /// <summary>
    /// Gets or sets whether the Catmull-Rom spline forms a closed curve.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the spline connects its end back to its beginning
    /// to form a closed curve; otherwise, <see langword="false"/>.
    /// The default value is <see langword="false"/>.
    /// </value>
    [Category(Categories.Design)]
    [DisplayName("Closed")]
    public bool IsClosed { get; set; }
}