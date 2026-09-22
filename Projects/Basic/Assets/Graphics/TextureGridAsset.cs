using Sachssoft.Engine.Graphics;
using System;
using System.IO;

namespace Sachssoft.Engine.Assets.Graphics;

/// <summary>
/// Represents a managed texture grid asset.
/// </summary>
public class TextureGridAsset
    : AssetBase<TextureGrid, TextureGridAssetDefinition>
{
    /// <summary>
    /// Initializes a new empty texture grid asset.
    /// </summary>
    /// <param name="id">The optional identifier of the asset.</param>
    /// <param name="class">The optional class of the asset.</param>
    public TextureGridAsset(string? id = null, string? @class = null)
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
    /// <param name="definition">The asset definition.</param>
    public TextureGridAsset(TextureGridAssetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Builds the runtime texture grid from the supplied stream.
    /// </summary>
    /// <param name="stream">The resource stream.</param>
    /// <returns>The created texture grid.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotImplementedException">
    /// Texture grid loading has not been implemented yet.
    /// </exception>
    protected override TextureGrid Build(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        throw new NotImplementedException(
            "Texture grid loading is not implemented yet.");
    }
}