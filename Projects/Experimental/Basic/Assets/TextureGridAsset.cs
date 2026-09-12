using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Experimental.Graphics;

namespace Sachssoft.Sasogine.Experimental.Assets.Graphics;

/// <summary>
/// Represents a managed texture grid asset.
/// </summary>
public class TextureGridAsset
    : AssetBase<TextureGrid, TextureGridAssetDefinition>
{
    /// <summary>
    /// Initializes a new empty texture grid asset.
    /// </summary>
    /// <param name="id">
    /// The optional identifier of the asset.
    /// </param>
    /// <param name="class">
    /// The optional class of the asset.
    /// </param>
    public TextureGridAsset(
        string? id = null,
        string? @class = null)
        : base(new TextureGridAssetDefinition
        {
            Id = id,
            Class = @class
        })
    {
    }

    /// <summary>
    /// Initializes a new texture grid asset from an existing definition.
    /// </summary>
    /// <param name="definition">
    /// The asset definition.
    /// </param>
    public TextureGridAsset(
        TextureGridAssetDefinition definition)
        : base(definition)
    {
    }
}