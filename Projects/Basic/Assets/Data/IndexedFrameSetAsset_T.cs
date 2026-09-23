using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Assets.Graphics;
using Sachssoft.Engine.Common;
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
    private readonly Func<string, TEnum>? _customConvert;
    private Texture2DAsset? _textureAsset;
    private Func<string, TEnum>? _convert;

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
        LoaderSource = loaderSource;
        _customConvert = convert;
    }

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

        Texture2DAsset textureAsset = _textureAsset ??
            throw new InvalidOperationException(
                "The texture asset could not be resolved.");

        Texture2D texture = textureAsset.GetOrLoad();

        ResourceSourceBase loaderSource = LoaderSource ??
            throw new InvalidOperationException(
                $"{nameof(LoaderSource)} is not configured.");

        FrameSetImporter importer = FrameSetImporter.Create(
            Definition.FormatType,
            loaderSource);

        return importer.ToIndexed(texture, _convert);
    }

    /// <summary>
    /// Applies the current asset definition and resolves its runtime dependencies.
    /// </summary>
    protected override void ConfigureFromDefinition()
    {
        base.ConfigureFromDefinition();

        _convert = _customConvert;

        if (Context is not null &&
            Definition.Texture is not null &&
            Context.Source.TryGet(
                Definition.Texture.Id,
                out IAsset? asset) &&
            asset is Texture2DAsset textureAsset)
        {
            _textureAsset = textureAsset;
        }

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
        if (!ReferenceEquals(asset, _textureAsset))
            return;

        if (IsLoaded)
            Reload();
    }

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