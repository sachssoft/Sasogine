using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Assets.Graphics;
using Sachssoft.Engine.Resources;
using Sachssoft.Engine.Resources.Importers;
using System;
using System.IO;
using System.Collections.Generic;

namespace Sachssoft.Engine.Assets.Data;

/// <summary>
/// Represents a managed asset that builds a keyed frame set.
/// </summary>
public class KeyedFrameSetAsset
    : AssetBase<KeyedFrameSet, KeyedFrameSetAssetDefinition>
{
    /// <summary>Stores additional single-page documents following the primary loader source.</summary>
    private readonly List<ResourceSourceBase> _additionalSources = new();

    /// <summary>Stores all resolved texture-page dependencies in atlas order.</summary>
    private Texture2DAsset[] _textureAssets = Array.Empty<Texture2DAsset>();

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
    /// Initializes a new keyed frame set asset using the specified resource
    /// source and texture asset reference.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    /// <param name="loaderSource">The resource source used to load the frame set.</param>
    /// <param name="textureAsset">The reference to the texture asset.</param>
    public KeyedFrameSetAsset(
        string? id,
        ResourceSourceBase? loaderSource,
        Reference<Texture2DAsset> textureAsset)
        : base(new KeyedFrameSetAssetDefinition
        {
            Id = id,
            Texture = textureAsset
        })
    {
        ArgumentNullException.ThrowIfNull(textureAsset);

        LoaderSource = loaderSource;
    }

    /// <summary>
    /// Initializes a new keyed frame set asset using the specified definition,
    /// resource source, and texture asset reference.
    /// </summary>
    /// <param name="definition">The asset definition.</param>
    /// <param name="loaderSource">The resource source used to load the frame set.</param>
    /// <param name="textureAsset">The reference to the texture asset.</param>
    public KeyedFrameSetAsset(
        KeyedFrameSetAssetDefinition definition,
        ResourceSourceBase? loaderSource,
        Reference<Texture2DAsset> textureAsset)
        : base(definition)
    {
        ArgumentNullException.ThrowIfNull(textureAsset);

        Definition.Texture = textureAsset;
        Definition.Textures = null;
        LoaderSource = loaderSource;
    }

    /// <summary>Creates an asset backed by an ordered collection of atlas textures.</summary>
    /// <param name="id">The optional asset identifier.</param>
    /// <param name="loaderSource">A multi-page atlas document or the first single-page document.</param>
    /// <param name="textureAssets">The ordered, nonempty texture page references.</param>
    public KeyedFrameSetAsset(string? id, ResourceSourceBase? loaderSource,
        IReadOnlyList<Reference<Texture2DAsset>> textureAssets)
        : base(new KeyedFrameSetAssetDefinition
        {
            Id = id,
            Textures = FrameSetAssetTextures.CopyReferences(textureAssets)
        })
    {
        LoaderSource = loaderSource;
    }

    /// <summary>Creates an asset with an existing definition and ordered atlas page references.</summary>
    /// <param name="definition">The asset definition.</param>
    /// <param name="loaderSource">A multi-page atlas document or the first single-page document.</param>
    /// <param name="textureAssets">The ordered, nonempty texture page references.</param>
    public KeyedFrameSetAsset(KeyedFrameSetAssetDefinition definition, ResourceSourceBase? loaderSource,
        IReadOnlyList<Reference<Texture2DAsset>> textureAssets)
        : base(definition)
    {
        Definition.Textures = FrameSetAssetTextures.CopyReferences(textureAssets);
        LoaderSource = loaderSource;
    }

    /// <summary>Gets additional single-page atlas sources ordered after the primary source.</summary>
    /// <remarks>Configure before loading. When nonempty, use one single-page document per texture; the primary source is page zero.</remarks>
    public IList<ResourceSourceBase> AdditionalSources => _additionalSources;

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

        Texture2D[] textures = FrameSetAssetTextures.Load(_textureAssets);

        ResourceSourceBase loaderSource = LoaderSource ??
            throw new InvalidOperationException(
                $"{nameof(LoaderSource)} is not configured.");

        return FrameSetAssetTextures.Import(Definition.FormatType, loaderSource, stream, _additionalSources, textures.Length)
            .ToKeyed(textures);
    }

    /// <summary>
    /// Applies the current asset definition and resolves its runtime dependencies.
    /// </summary>
    protected override void ConfigureFromDefinition()
    {
        base.ConfigureFromDefinition();

        _textureAssets = FrameSetAssetTextures.Resolve(Context, Definition.Texture, Definition.Textures);
    }

    /// <inheritdoc/>
    protected override void OnReferenceChanged(IAsset asset)
    {
        if (IsLoaded && FrameSetAssetTextures.Contains(_textureAssets, asset)) Reload();
    }
}
