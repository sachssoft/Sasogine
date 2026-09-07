using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Assets.Graphics;

/// <summary>
/// Represents a managed 3D model asset for the Sasogine graphics system.
/// </summary>
/// <remarks>
/// <see cref="ModelAsset"/> provides a managed <see cref="Model"/> resource
/// configured through a <see cref="ModelAssetDefinition"/>.
/// </remarks>
public class ModelAsset : AssetBase<Model, ModelAssetDefinition>
{
    /// <summary>
    /// Initializes a new empty instance of the <see cref="ModelAsset"/> class.
    /// </summary>
    /// <param name="id">
    /// The optional identifier of the asset.
    /// </param>
    /// <param name="class">
    /// The optional class of the asset.
    /// </param>
    public ModelAsset(
        string? id = null,
        string? @class = null)
        : base(new ModelAssetDefinition
        {
            Id = id,
            Class = @class,
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelAsset"/> class
    /// using the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The asset definition containing the model configuration.
    /// </param>
    public ModelAsset(
        ModelAssetDefinition definition)
        : base(definition)
    {
    }
}