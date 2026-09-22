using Sachssoft.Engine.Common.Collections;
using Sachssoft.Engine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Engine.Assets.Graphics;

/// <summary>
/// Defines a texture grid asset.
/// </summary>
public class TextureGridAssetDefinition : AssetDefinitionBase
{
    /// <summary>
    /// Gets or sets the number of columns in the texture grid.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Columns")]
    public int Columns { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of rows in the texture grid.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Rows")]
    public int Rows { get; set; } = 1;

    /// <summary>
    /// Gets or sets the cells contained in the texture grid.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Cells")]
    public TrackableCollection<TextureGridAssetCellDefinition> Cells { get; } = new();
}