using Sachssoft.Engine.Common;
using Sachssoft.Engine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines the configurable state shared by all vector segments.
/// </summary>
public class VectorSegmentDefinition : IDefinition
{
    /// <summary>
    /// Gets the definition of the endpoint node of the vector segment.
    /// </summary>
    /// <value>
    /// The <see cref="VectorNodeDefinition"/> that defines the endpoint
    /// of the segment.
    /// </value>
    [Category(Categories.Design)]
    [DisplayName("Node")]
    public VectorNodeDefinition Node { get; } = new();
}