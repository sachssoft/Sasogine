using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector;

/// <summary>
/// Defines the configurable state of a vector node.
/// </summary>
public sealed class VectorNodeDefinition : IDefinition
{
    /// <summary>
    /// Gets or sets the position of the vector node.
    /// </summary>
    /// <value>
    /// The two-dimensional position of the node.
    /// </value>
    [Category(Categories.Transform)]
    [DisplayName("Position")]
    public Point2 Position { get; set; }

    /// <summary>
    /// Gets or sets whether the vector node is currently selected.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the node is selected; otherwise,
    /// <see langword="false"/>.
    /// </value>
    [Category(Categories.Edit)]
    [DisplayName("Selected")]
    public bool IsSelected { get; set; }
}