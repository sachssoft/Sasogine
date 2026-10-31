using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Assets.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Resources;
using Sachssoft.Engine.Resources.Importers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Engine.Assets.Data;

/// <summary>
/// Represents an asset that loads an indexed frame set using enum values as
/// frame indices.
/// </summary>
/// <typeparam name="TEnum">The enum type used to identify frames.</typeparam>
public class IndexedFrameSetAsset<TEnum>
    : AssetBase<IndexedFrameSet<TEnum>, IndexedFrameSetDefinition>
    where TEnum : struct, Enum
{
    /// <summary>Stores an explicitly supplied frame-name converter.</summary>
    private readonly Func<string, TEnum>? _customConvert;

    /// <summary>Stores additional single-page documents following the primary loader source.</summary>
    private readonly List<ResourceSourceBase> _additionalSources = new();

    /// <summary>Stores the configured converter or the converter derived from index mappings.</summary>
    private Func<string, TEnum>? _convert;

    /// <summary>Stores all resolved texture-page dependencies in atlas order.</summary>
    private Texture2DAsset[] _textureAssets = Array.Empty<Texture2DAsset>();

    /// <summary>
    /// Initializes a new indexed frame set asset.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    public IndexedFrameSetAsset(string? id)
        : base(new IndexedFrameSetDefinition { Id = id })
    {
    }

    /// <summary>
    /// Initializes a new indexed frame set asset using the specified definition.
    /// </summary>
    /// <param name="definition">The asset definition.</param>
    public IndexedFrameSetAsset(IndexedFrameSetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Initializes a new indexed frame set asset using the specified resource
    /// source, texture asset reference, and optional enum-to-name mapping.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    /// <param name="loaderSource">The resource source used to load the frame set.</param>
    /// <param name="textureAsset">The reference to the texture asset used by the frame set.</param>
    /// <param name="indexMapping">The optional enum-to-name mapping.</param>
    public IndexedFrameSetAsset(
        string? id,
        ResourceSourceBase? loaderSource,
        Reference<Texture2DAsset> textureAsset,
        IDictionary<TEnum, string>? indexMapping = null)
        : base(new IndexedFrameSetDefinition
        {
            Id = id,
            Texture = textureAsset,
            IndexMapping = ConvertToMapping(indexMapping)
        })
    {
        ArgumentNullException.ThrowIfNull(textureAsset);
        LoaderSource = loaderSource;
    }

    /// <summary>
    /// Initializes a new indexed frame set asset using the specified definition,
    /// resource source, texture asset reference, and optional enum-to-name mapping.
    /// </summary>
    /// <param name="definition">The asset definition.</param>
    /// <param name="loaderSource">The resource source used to load the frame set.</param>
    /// <param name="textureAsset">The reference to the texture asset used by the frame set.</param>
    /// <param name="indexMapping">The optional enum-to-name mapping.</param>
    public IndexedFrameSetAsset(
        IndexedFrameSetDefinition definition,
        ResourceSourceBase? loaderSource,
        Reference<Texture2DAsset> textureAsset,
        IDictionary<TEnum, string>? indexMapping = null)
        : base(definition)
    {
        ArgumentNullException.ThrowIfNull(textureAsset);

        Definition.Texture = textureAsset;
        Definition.Textures = null;

        if (indexMapping is not null)
            Definition.IndexMapping = ConvertToMapping(indexMapping);

        LoaderSource = loaderSource;
    }

    /// <summary>
    /// Initializes a new indexed frame set asset using the specified resource
    /// source, texture asset reference, and frame index converter.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    /// <param name="loaderSource">The resource source used to load the frame set.</param>
    /// <param name="textureAsset">The reference to the texture asset used by the frame set.</param>
    /// <param name="convert">The function used to convert frame names to enum values.</param>
    public IndexedFrameSetAsset(
        string? id,
        ResourceSourceBase? loaderSource,
        Reference<Texture2DAsset> textureAsset,
        Func<string, TEnum> convert)
        : base(new IndexedFrameSetDefinition
        {
            Id = id,
            Texture = textureAsset
        })
    {
        ArgumentNullException.ThrowIfNull(textureAsset);
        ArgumentNullException.ThrowIfNull(convert);

        LoaderSource = loaderSource;
        _customConvert = convert;
    }

    /// <summary>
    /// Initializes a new indexed frame set asset using the specified definition,
    /// resource source, texture asset reference, and frame index converter.
    /// </summary>
    /// <param name="definition">The asset definition.</param>
    /// <param name="loaderSource">The resource source used to load the frame set.</param>
    /// <param name="textureAsset">The reference to the texture asset used by the frame set.</param>
    /// <param name="convert">The function used to convert frame names to enum values.</param>
    public IndexedFrameSetAsset(
        IndexedFrameSetDefinition definition,
        ResourceSourceBase? loaderSource,
        Reference<Texture2DAsset> textureAsset,
        Func<string, TEnum> convert)
        : base(definition)
    {
        ArgumentNullException.ThrowIfNull(textureAsset);
        ArgumentNullException.ThrowIfNull(convert);

        Definition.Texture = textureAsset;
        Definition.Textures = null;
        LoaderSource = loaderSource;
        _customConvert = convert;
    }

    /// <summary>Creates an asset backed by an ordered collection of atlas textures.</summary>
    /// <param name="id">The optional asset identifier.</param>
    /// <param name="loaderSource">A multi-page atlas document or the first single-page document.</param>
    /// <param name="textureAssets">The ordered, nonempty texture page references.</param>
    /// <param name="convert">The optional frame-name converter.</param>
    public IndexedFrameSetAsset(string? id, ResourceSourceBase? loaderSource,
        IReadOnlyList<Reference<Texture2DAsset>> textureAssets, Func<string, TEnum>? convert = null)
        : base(new IndexedFrameSetDefinition
        {
            Id = id,
            Textures = FrameSetAssetTextures.CopyReferences(textureAssets)
        })
    {
        LoaderSource = loaderSource;
        _customConvert = convert;
    }

    /// <summary>Creates an asset with an existing definition and ordered atlas page references.</summary>
    /// <param name="definition">The asset definition.</param>
    /// <param name="loaderSource">A multi-page atlas document or the first single-page document.</param>
    /// <param name="textureAssets">The ordered, nonempty texture page references.</param>
    /// <param name="convert">The optional frame-name converter.</param>
    public IndexedFrameSetAsset(IndexedFrameSetDefinition definition, ResourceSourceBase? loaderSource,
        IReadOnlyList<Reference<Texture2DAsset>> textureAssets, Func<string, TEnum>? convert = null)
        : base(definition)
    {
        Definition.Textures = FrameSetAssetTextures.CopyReferences(textureAssets);
        LoaderSource = loaderSource;
        _customConvert = convert;
    }

    /// <summary>Gets additional single-page atlas sources ordered after the primary source.</summary>
    /// <remarks>Configure before loading. When nonempty, use one single-page document per texture; the primary source is page zero.</remarks>
    public IList<ResourceSourceBase> AdditionalSources => _additionalSources;

    /// <summary>
    /// Resolves the default definition used by this asset.
    /// </summary>
    /// <returns>A new <see cref="IndexedFrameSetDefinition"/> instance.</returns>
    protected override IndexedFrameSetDefinition ResolveDefinition()
    {
        return new IndexedFrameSetDefinition();
    }

    /// <summary>
    /// Builds the runtime indexed frame set.
    /// </summary>
    /// <param name="stream">The resource stream.</param>
    /// <returns>The created indexed frame set.</returns>
    protected override IndexedFrameSet<TEnum> Build(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Texture2D[] textures = FrameSetAssetTextures.Load(_textureAssets);

        ResourceSourceBase loaderSource = LoaderSource ??
            throw new InvalidOperationException(
                $"{nameof(LoaderSource)} is not configured.");

        return FrameSetAssetTextures.Import(Definition.FormatType, loaderSource, stream, _additionalSources, textures.Length)
            .ToIndexed(textures, _convert);
    }

    /// <summary>
    /// Applies the current asset definition and resolves its runtime dependencies.
    /// </summary>
    protected override void ConfigureFromDefinition()
    {
        base.ConfigureFromDefinition();

        _convert = _customConvert;

        _textureAssets = FrameSetAssetTextures.Resolve(Context, Definition.Texture, Definition.Textures);

        if (_convert is null &&
            Definition.IndexMapping is { Count: > 0 } mapping)
        {
            _convert = name =>
            {
                IndexMapping? entry = mapping.FirstOrDefault(item =>
                    string.Equals(
                        item.Name,
                        name,
                        StringComparison.OrdinalIgnoreCase));

                if (entry is not null)
                    return (TEnum)Enum.ToObject(typeof(TEnum), entry.Index);

                return Enum.Parse<TEnum>(name, ignoreCase: true);
            };
        }
    }

    /// <summary>
    /// Handles changes to assets that affect this asset's runtime dependencies.
    /// </summary>
    /// <param name="asset">The asset affected by the reference change.</param>
    protected override void OnReferenceChanged(IAsset asset)
    {
        if (!FrameSetAssetTextures.Contains(_textureAssets, asset))
            return;

        if (IsLoaded)
            Reload();
    }

    /// <summary>Converts an optional enum-to-name map into the definition's integer mappings.</summary>
    /// <param name="mapping">The enum-to-name map, or null.</param>
    /// <returns>The converted mapping list, or null.</returns>
    private static IList<IndexMapping>? ConvertToMapping(
        IDictionary<TEnum, string>? mapping)
    {
        if (mapping is null)
            return null;

        return mapping.Select(pair => new IndexMapping
        {
            Index = Convert.ToInt32(pair.Key),
            Name = pair.Value
        }).ToList();
    }
}
