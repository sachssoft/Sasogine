using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Resources;

/// <summary>Validates and snapshots borrowed atlas textures without taking ownership of their lifetime.</summary>
internal static class FrameSetTextures
{
    /// <summary>Wraps a borrowed texture in a read-only page collection.</summary>
    /// <param name="texture">The non-null default texture.</param>
    /// <returns>A read-only collection containing page zero.</returns>
    internal static IReadOnlyList<Texture2D> Create(Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        return Array.AsReadOnly(new[] { texture });
    }

    /// <summary>Copies page references so later changes to the caller's collection cannot invalidate frames.</summary>
    /// <param name="textures">A nonempty ordered collection of non-null borrowed textures.</param>
    /// <returns>An independently owned, read-only collection of texture references.</returns>
    internal static IReadOnlyList<Texture2D> Create(IReadOnlyList<Texture2D> textures)
    {
        ArgumentNullException.ThrowIfNull(textures);
        if (textures.Count == 0)
            throw new ArgumentException("At least one atlas texture is required.", nameof(textures));

        var copy = new Texture2D[textures.Count];
        for (int i = 0; i < copy.Length; i++)
            copy[i] = textures[i] ?? throw new ArgumentException("Atlas textures must not contain null.", nameof(textures));
        return Array.AsReadOnly(copy);
    }

    /// <summary>Checks a page index before inserting a frame.</summary>
    /// <param name="textures">The registered atlas textures.</param>
    /// <param name="textureIndex">The zero-based page index to validate.</param>
    internal static void ValidateIndex(IReadOnlyList<Texture2D> textures, int textureIndex)
    {
        if ((uint)textureIndex >= (uint)textures.Count)
            throw new ArgumentOutOfRangeException(nameof(textureIndex), textureIndex, "No atlas texture exists at this index.");
    }
}

