using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Assets.Graphics;
using Sachssoft.Engine.Resources;
using Sachssoft.Engine.Resources.Importers;
using System;
using System.IO;

namespace Sachssoft.Engine.Assets.Data;

/// <summary>
/// Represents a managed asset that builds a keyed frame set.
/// </summary>
public class KeyedFrameSetAsset
    : AssetBase<KeyedFrameSet, KeyedFrameSetAssetDefinition>
{
    private Texture2D? _texture;

    /// <summary>
    /// Initializes a new keyed frame set asset.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    public KeyedFrameSetAsset(string? id)
        : base(new KeyedFrameSetAssetDefinition { Id = id })
    {
    }

    /// <summary>
    /// Initializes a new keyed frame set asset using the specified definition.
    /// </summary>
    /// <param name="definition">The asset definition.</param>
    public KeyedFrameSetAsset(KeyedFrameSetAssetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Initializes a new keyed frame set asset using the specified resource source.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    /// <param name="loaderSource">The resource source used to load the frame set.</param>
    public KeyedFrameSetAsset(string? id, ResourceSourceBase? loaderSource)
        : base(new KeyedFrameSetAssetDefinition { Id = id })
    {
        LoaderSource = loaderSource;
    }

    /// <summary>
    /// Initializes a new keyed frame set asset using the specified definition
    /// and resource source.
    /// </summary>
    /// <param name="definition">The asset definition.</param>
    /// <param name="loaderSource">The resource source used to load the frame set.</param>
    public KeyedFrameSetAsset(
        KeyedFrameSetAssetDefinition definition,
        ResourceSourceBase? loaderSource)
        : base(definition)
    {
        LoaderSource = loaderSource;
    }

    /// <summary>
    /// Builds the runtime keyed frame set.
    /// </summary>
    /// <param name="stream">The resource stream.</param>
    /// <returns>The created keyed frame set.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The required texture or loader source is unavailable.
    /// </exception>
    protected override KeyedFrameSet Build(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Texture2D texture = _texture ??
            throw new InvalidOperationException(
                "The texture asset could not be resolved.");

        ResourceSourceBase loaderSource = LoaderSource ??
            throw new InvalidOperationException(
                $"{nameof(LoaderSource)} is not configured.");

        FrameSetImporter importer = FrameSetImporter.Create(
            Definition.FormatType,
            loaderSource);

        return importer.ToKeyed(texture);
    }

    /// <summary>
    /// Applies the current asset definition and resolves its runtime dependencies.
    /// </summary>
    protected override void ConfigureFromDefinition()
    {
        base.ConfigureFromDefinition();

        _texture = null;

        if (Context is not null &&
            Definition.Texture is not null &&
            Context.Source.TryGet(Definition.Texture.Id, out IAsset? asset) &&
            asset is Texture2DAsset textureAsset)
        {
            _texture = textureAsset.GetOrLoad();
        }
    }
}