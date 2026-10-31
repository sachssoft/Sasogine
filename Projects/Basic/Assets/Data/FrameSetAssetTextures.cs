using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Assets.Graphics;
using Sachssoft.Engine.Resources;
using Sachssoft.Engine.Resources.Importers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sachssoft.Engine.Assets.Data;

/// <summary>Shares explicit atlas dependency resolution and ordered page import between frame-set assets.</summary>
internal static class FrameSetAssetTextures
{
    /// <summary>Copies ordered references without allowing missing texture pages.</summary>
    /// <param name="textures">The nonempty texture asset references.</param>
    /// <returns>An independent reference list.</returns>
    internal static IList<Reference<Texture2DAsset>> CopyReferences(IReadOnlyList<Reference<Texture2DAsset>> textures)
    {
        ArgumentNullException.ThrowIfNull(textures);
        if (textures.Count == 0) throw new ArgumentException("At least one texture reference is required.", nameof(textures));
        var result = new List<Reference<Texture2DAsset>>(textures.Count);
        for (int i = 0; i < textures.Count; i++)
            result.Add(textures[i] ?? throw new ArgumentException("Texture references must not contain null.", nameof(textures)));
        return result;
    }

    /// <summary>Resolves every page, using the legacy texture only when no page list is configured.</summary>
    /// <param name="context">The asset context, or null before initialization.</param>
    /// <param name="texture">The legacy single-texture reference.</param>
    /// <param name="textures">The optional ordered page references, taking precedence over the legacy reference.</param>
    /// <returns>The resolved dependencies, or an empty array before configuration is complete.</returns>
    internal static Texture2DAsset[] Resolve(AssetContext? context, Reference<Texture2DAsset>? texture,
        IList<Reference<Texture2DAsset>>? textures)
    {
        if (context is null) return Array.Empty<Texture2DAsset>();
        int count = textures is { Count: > 0 } ? textures.Count : texture is null ? 0 : 1;
        var result = new Texture2DAsset[count];
        for (int i = 0; i < count; i++)
        {
            Reference<Texture2DAsset>? reference = textures is { Count: > 0 } ? textures[i] : texture;
            if (reference is null || string.IsNullOrWhiteSpace(reference.Id) ||
                !context.Source.TryGet(reference.Id, out IAsset? asset) || asset is not Texture2DAsset page)
                throw new InvalidOperationException($"Texture asset for atlas page {i} could not be resolved.");
            result[i] = page;
        }
        return result;
    }

    /// <summary>Loads all resolved texture pages before constructing the runtime frame set.</summary>
    /// <param name="assets">The resolved page dependencies.</param>
    /// <returns>The textures in atlas page order.</returns>
    internal static Texture2D[] Load(IReadOnlyList<Texture2DAsset> assets)
    {
        if (assets.Count == 0) throw new InvalidOperationException("The frame set has no resolved texture assets.");
        var result = new Texture2D[assets.Count];
        for (int i = 0; i < result.Length; i++) result[i] = assets[i].GetOrLoad();
        return result;
    }

    /// <summary>Checks whether a changed dependency belongs to any atlas page.</summary>
    /// <param name="assets">The resolved page dependencies.</param>
    /// <param name="changed">The changed asset.</param>
    /// <returns>Whether the changed asset is a page dependency.</returns>
    internal static bool Contains(IReadOnlyList<Texture2DAsset> assets, IAsset changed)
    {
        for (int i = 0; i < assets.Count; i++)
            if (ReferenceEquals(assets[i], changed)) return true;
        return false;
    }

    /// <summary>Reads a multi-page document or one single-page document per explicitly configured source.</summary>
    /// <param name="format">The format shared by all documents.</param>
    /// <param name="source">The primary document source.</param>
    /// <param name="stream">The already-open primary stream, which remains owned by the asset loader.</param>
    /// <param name="additionalSources">Optional single-page sources following the primary page.</param>
    /// <param name="textureCount">The number of configured textures.</param>
    /// <returns>Imported entries whose page indices match the configured texture order.</returns>
    internal static IEnumerable<FrameSetEntry> Import(DocumentFormatType format, ResourceSourceBase source,
        Stream stream, IReadOnlyList<ResourceSourceBase> additionalSources, int textureCount)
    {
        if (additionalSources.Count > 0 && textureCount != additionalSources.Count + 1)
            throw new InvalidOperationException("One texture is required for each atlas document source.");
        foreach (FrameSetEntry entry in FrameSetImporter.Create(format, source).Import(stream))
        {
            if (additionalSources.Count > 0 && entry.TextureIndex != 0)
                throw new InvalidDataException("AdditionalSources requires one single-page document per texture.");
            yield return entry;
        }
        for (int i = 0; i < additionalSources.Count; i++)
        {
            ResourceSourceBase page = additionalSources[i] ?? throw new InvalidOperationException("Atlas sources must not contain null.");
            foreach (FrameSetEntry entry in FrameSetImporter.Create(format, page).Import())
            {
                if (entry.TextureIndex != 0)
                    throw new InvalidDataException("AdditionalSources requires one single-page document per texture.");
                entry.TextureIndex = i + 1;
                yield return entry;
            }
        }
    }
}
