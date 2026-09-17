using System.Collections.Generic;

namespace Sachssoft.Sasogine.Assets.Graphics;

/// <summary>
/// Defines a texture grid asset.
/// </summary>
public class TextureGridAssetDefinition : AssetDefinitionBase<TextureGridAsset>
{
    /// <summary>
    /// Gets or sets the number of columns in the texture grid.
    /// </summary>
    public int Columns { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of rows in the texture grid.
    /// </summary>
    public int Rows { get; set; } = 1;

    /// <summary>
    /// Gets or sets the cells contained in the texture grid.
    /// </summary>
    public IList<TextureGridAssetCellDefinition> Cells { get; set; }
        = new List<TextureGridAssetCellDefinition>();
}