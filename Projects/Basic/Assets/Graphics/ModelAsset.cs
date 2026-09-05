using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Assets.Graphics;

/// <summary>
/// Represents a 3D model asset.
/// </summary>
public class ModelAsset : AssetBase<Model, ModelAssetDefinition>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelAsset"/> class.
    /// </summary>
    public ModelAsset()
        : base(new ModelAssetDefinition())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelAsset"/> class
    /// using the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to configure the model asset.
    /// </param>
    public ModelAsset(ModelAssetDefinition definition)
        : base(definition)
    {
    }
}