using Sachssoft.Engine.Components.Definitions;
using System.ComponentModel;

namespace Sachssoft.Engine.Assets.Graphics;

/// <summary>
/// Defines a cell within a texture grid asset.
/// </summary>
public class TextureGridAssetCellDefinition
{
    /// <summary>
    /// Gets or sets the column index of the cell.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Column")]
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets the row index of the cell.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Row")]
    public int Row { get; set; }

    /// <summary>
    /// Gets or sets the resource reference associated with the cell.
    /// </summary>
    /// <remarks>
    /// The reference does not define how the resource is resolved.
    /// It may represent an asset identifier, resource key, path, or
    /// another reference supported by the asset system.
    /// </remarks>
    [Category(Categories.Common)]
    [DisplayName("Reference")]
    public string? Reference { get; set; }
}