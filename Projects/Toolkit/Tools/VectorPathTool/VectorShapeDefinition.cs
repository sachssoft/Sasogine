using Sachssoft.Engine.Collections;
using Sachssoft.Engine.Components.Definitions;
using System.ComponentModel;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines the configurable state of a vector shape.
/// </summary>
public class VectorShapeDefinition : IDefinition
{
    /// <summary>
    /// Gets the collection of vector path definitions that make up the shape.
    /// </summary>
    /// <remarks>
    /// The paths are managed through specialized vector editing tools rather
    /// than directly through the property inspector.
    /// </remarks>
    [Browsable(false)]
    public TrackableCollection<VectorPathDefinition> Paths { get; } = [];

    /// <summary>
    /// Gets or sets whether the vector shape is locked for editing.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the shape is locked and should not be modified
    /// through editing operations; otherwise, <see langword="false"/>.
    /// The default value is <see langword="false"/>.
    /// </value>
    [Category(Categories.Design)]
    [DisplayName("Is Locked")]
    public bool IsLocked { get; set; }
}