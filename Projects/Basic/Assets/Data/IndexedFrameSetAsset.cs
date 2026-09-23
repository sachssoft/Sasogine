using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Assets.Graphics;
using Sachssoft.Engine.Common;
using Sachssoft.Engine.Resources;
using Sachssoft.Engine.Resources.Importers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sachssoft.Engine.Assets.Data;

/// <summary>
/// Represents an asset that loads a frame set using integer indices and supports
/// creating strongly typed enum-indexed frame set instances.
/// </summary>
public class IndexedFrameSetAsset
    : AssetBase<IndexedFrameSet, IndexedFrameSetDefinition>
{
    private Texture2DAsset? _textureAsset;

    /// <summary>
    /// Initializes a new indexed frame set asset.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    public IndexedFrameSetAsset(string? id)
        : base(new IndexedFrameSetDefinition
        {
            Id = id
        })
    {
    }

    /// <summary>
    /// Initializes a new indexed frame set asset using the specified definition.
    /// </summary>
    /// <param name="definition">The asset definition.</param>
    public IndexedFrameSetAsset(
        IndexedFrameSetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Initializes a new indexed frame set asset using the specified resource
    /// source and texture asset reference.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    /// <param name="loaderSource">
    /// The resource source used to load the frame set.
    /// </param>
    /// <param name="textureAsset">
    /// The reference to the texture asset used by the frame set.
    /// </param>
    public IndexedFrameSetAsset(
        string? id,
        ResourceSourceBase? loaderSource,
        Reference<Texture2DAsset> textureAsset)
        : base(new IndexedFrameSetDefinition
        {
            Id = id,
            Texture = textureAsset
        })
    {
        ArgumentNullException.ThrowIfNull(textureAsset);

        LoaderSource = loaderSource;
    }

    /// <summary>
    /// Initializes a new indexed frame set asset using the specified definition,
    /// resource source, and texture asset reference.
    /// </summary>
    /// <param name="definition">The asset definition.</param>
    /// <param name="loaderSource">
    /// The resource source used to load the frame set.
    /// </param>
    /// <param name="textureAsset">
    /// The reference to the texture asset used by the frame set.
    /// </param>
    public IndexedFrameSetAsset(
        IndexedFrameSetDefinition definition,
        ResourceSourceBase? loaderSource,
        Reference<Texture2DAsset> textureAsset)
        : base(definition)
    {
        ArgumentNullException.ThrowIfNull(textureAsset);

        Definition.Texture = textureAsset;
        LoaderSource = loaderSource;
    }

    /// <summary>
    /// Gets a strongly typed enum-indexed frame set instance.
    /// </summary>
    /// <typeparam name="TEnum">
    /// The enum type used to identify frames.
    /// </typeparam>
    /// <returns>
    /// A strongly typed frame set using <typeparamref name="TEnum"/> as its
    /// frame index type.
    /// </returns>
    public IndexedFrameSet<TEnum> GetInstance<TEnum>()
        where TEnum : struct, Enum
    {
        return GetOrLoad()
            .ToEnumIndexed<TEnum>();
    }

    /// <summary>
    /// Resolves the default definition used by this asset.
    /// </summary>
    /// <returns>
    /// A new <see cref="IndexedFrameSetDefinition"/> instance.
    /// </returns>
    protected override IndexedFrameSetDefinition ResolveDefinition()
    {
        return new IndexedFrameSetDefinition();
    }

    /// <summary>
    /// Builds the runtime indexed frame set from the configured frame set document.
    /// </summary>
    /// <param name="stream">
    /// The resource stream associated with the asset.
    /// </param>
    /// <returns>
    /// The created indexed frame set.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The texture asset or loader source could not be resolved.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// A frame name could not be resolved to a configured index.
    /// </exception>
    protected override IndexedFrameSet Build(
        Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Texture2DAsset textureAsset = _textureAsset ??
            throw new InvalidOperationException(
                "The texture asset could not be resolved.");

        Texture2D texture =
            textureAsset.GetOrLoad();

        ResourceSourceBase loaderSource = LoaderSource ??
            throw new InvalidOperationException(
                $"{nameof(LoaderSource)} is not configured.");

        FrameSetImporter importer =
            FrameSetImporter.Create(
                Definition.FormatType,
                loaderSource);

        IndexedFrameSet frameSet =
            new(texture);

        Dictionary<string, int>? indexMapping = null;

        if (Definition.IndexMapping is not null)
        {
            indexMapping = new Dictionary<string, int>(
                Definition.IndexMapping.Count,
                StringComparer.Ordinal);

            foreach (IndexMapping mapping in Definition.IndexMapping)
            {
                if (mapping.Name is null)
                {
                    throw new InvalidDataException(
                        $"Index mapping '{mapping.Index}' does not define a frame name.");
                }

                indexMapping.Add(
                    mapping.Name,
                    mapping.Index);
            }
        }

        int index = 0;

        foreach (FrameSetEntry entry in importer.Import())
        {
            int frameIndex;

            if (indexMapping is not null)
            {
                if (!indexMapping.TryGetValue(
                        entry.Name,
                        out frameIndex))
                {
                    throw new InvalidDataException(
                        $"No index mapping exists for frame '{entry.Name}'.");
                }
            }
            else
            {
                frameIndex = index;
            }

            frameSet.Add(
                frameIndex,
                new Point(
                    entry.X,
                    entry.Y),
                new PixelSize2(
                    entry.Width,
                    entry.Height));

            index++;
        }

        return frameSet;
    }

    /// <summary>
    /// Applies the current asset definition and resolves its runtime dependencies.
    /// </summary>
    protected override void ConfigureFromDefinition()
    {
        base.ConfigureFromDefinition();

        if (Context is not null &&
            Definition.Texture is not null &&
            Context.Source.TryGet(
                Definition.Texture.Id,
                out IAsset? asset) &&
            asset is Texture2DAsset textureAsset)
        {
            _textureAsset = textureAsset;
        }
    }

    /// <summary>
    /// Handles changes to assets that affect this asset's runtime dependencies.
    /// </summary>
    /// <param name="asset">
    /// The asset affected by the reference change.
    /// </param>
    protected override void OnReferenceChanged(
        IAsset asset)
    {
        if (!ReferenceEquals(
                asset,
                _textureAsset))
        {
            return;
        }

        if (IsLoaded)
            Reload();
    }
}