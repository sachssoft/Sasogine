using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Engine.Resources;

/// <summary>Stores tile frames across one or more borrowed atlas textures.</summary>
/// <remarks>Textures are not disposed by this collection. Use each frame's texture index when rendering.</remarks>
public sealed class SequentialTileFrameSet : ITileFrameSet
{
    /// <summary>Stores frames in insertion order.</summary>
    private readonly List<TileFrameData> _frames = new();

    /// <summary>Creates a frame set backed by one atlas texture.</summary>
    /// <param name="tileSize">The common tile size in pixels.</param>
    /// <param name="texture">The borrowed texture containing page zero.</param>
    public SequentialTileFrameSet(PixelSize2 tileSize, Texture2D texture)
    {
        TileSize = tileSize;
        Textures = FrameSetTextures.Create(texture);
    }

    /// <summary>Creates a frame set backed by multiple ordered atlas textures.</summary>
    /// <param name="tileSize">The common tile size in pixels.</param>
    /// <param name="textures">The nonempty, ordered atlas pages. Their references are copied.</param>
    public SequentialTileFrameSet(PixelSize2 tileSize, IReadOnlyList<Texture2D> textures)
    {
        TileSize = tileSize;
        Textures = FrameSetTextures.Create(textures);
    }

    /// <summary>Gets the ordered atlas pages without transferring texture ownership.</summary>
    public IReadOnlyList<Texture2D> Textures
    {
        get;
    }

    /// <summary>Gets page zero for compatibility with single-texture frame sets.</summary>
    /// <remarks>For multiple pages, use <see cref="GetTexture(TileFrameData)"/> or the frame's texture index.</remarks>
    public Texture2D Texture => Textures[0];

    /// <summary>Gets the common tile size in pixels.</summary>
    public PixelSize2 TileSize
    {
        get;
    }

    /// <summary>Gets the number of registered frames.</summary>
    public int Count => _frames.Count;

    /// <inheritdoc/>
    IEnumerable<object> ITileFrameSet.Keys => Enumerable.Range(0, _frames.Count).Cast<object>();

    /// <summary>Gets the frame associated with the specified key.</summary>
    /// <param name="index">The registered frame key.</param>
    /// <returns>The frame, including its atlas page index.</returns>
    public TileFrameData this[int index] => _frames[index];

    /// <inheritdoc/>
    TileFrameData ITileFrameSet.this[object key] => this[(int)key];

    /// <summary>Gets the atlas texture referenced by a frame.</summary>
    /// <param name="frame">The frame whose page should be resolved.</param>
    /// <returns>The corresponding borrowed texture.</returns>
    public Texture2D GetTexture(TileFrameData frame)
    {
        FrameSetTextures.ValidateIndex(Textures, frame.TextureIndex);
        return Textures[frame.TextureIndex];
    }

    /// <summary>Gets the atlas texture for a registered frame key.</summary>
    /// <param name="index">The frame key.</param>
    /// <returns>The corresponding borrowed texture.</returns>
    public Texture2D GetTexture(int index) => GetTexture(this[index]);

    /// <summary>Adds a frame on atlas page zero.</summary>
    /// <param name="cell">The atlas cell coordinate.</param>
    public void Add(Coordinate2 cell) => Add(cell, 0);

    /// <summary>Adds a frame on the specified atlas page.</summary>
    /// <param name="cell">The atlas cell coordinate.</param>
    /// <param name="textureIndex">The zero-based index into <see cref="Textures"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The texture index is not registered.</exception>
    public void Add(Coordinate2 cell, int textureIndex)
    {
        FrameSetTextures.ValidateIndex(Textures, textureIndex);
        _frames.Add(new TileFrameData(TileSize, cell, textureIndex));
    }

    /// <summary>Returns an enumerator over the registered frames.</summary>
    /// <returns>The frame enumerator.</returns>
    public IEnumerator<TileFrameData> GetEnumerator() => _frames.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}