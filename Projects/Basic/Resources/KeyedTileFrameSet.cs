using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Engine.Resources;

/// <summary>Stores tile frames across one or more borrowed atlas textures.</summary>
/// <remarks>Textures are not disposed by this collection. Use each frame's texture index when rendering.</remarks>
public sealed class KeyedTileFrameSet : ITileFrameSet
{
    /// <summary>Stores frames by their strongly typed key.</summary>
    private readonly Dictionary<string, TileFrameData> _frames = new();

    /// <summary>Creates a frame set backed by one atlas texture.</summary>
    /// <param name="tileSize">The common tile size in pixels.</param>
    /// <param name="texture">The borrowed texture containing page zero.</param>
    public KeyedTileFrameSet(PixelSize2 tileSize, Texture2D texture)
    {
        TileSize = tileSize;
        Textures = FrameSetTextures.Create(texture);
    }

    /// <summary>Creates a frame set backed by multiple ordered atlas textures.</summary>
    /// <param name="tileSize">The common tile size in pixels.</param>
    /// <param name="textures">The nonempty, ordered atlas pages. Their references are copied.</param>
    public KeyedTileFrameSet(PixelSize2 tileSize, IReadOnlyList<Texture2D> textures)
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

    /// <summary>Gets the registered frame keys.</summary>
    public IEnumerable<string> Keys => _frames.Keys;

    /// <inheritdoc/>
    IEnumerable<object> ITileFrameSet.Keys => _frames.Keys;

    /// <summary>Gets the frame associated with the specified key.</summary>
    /// <param name="key">The registered frame key.</param>
    /// <returns>The frame, including its atlas page index.</returns>
    public TileFrameData this[string key] => _frames[key];

    /// <inheritdoc/>
    TileFrameData ITileFrameSet.this[object key] => this[(string)key];

    /// <summary>Gets the atlas texture referenced by a frame.</summary>
    /// <param name="frame">The frame whose page should be resolved.</param>
    /// <returns>The corresponding borrowed texture.</returns>
    public Texture2D GetTexture(TileFrameData frame)
    {
        FrameSetTextures.ValidateIndex(Textures, frame.TextureIndex);
        return Textures[frame.TextureIndex];
    }

    /// <summary>Gets the atlas texture for a registered frame key.</summary>
    /// <param name="key">The frame key.</param>
    /// <returns>The corresponding borrowed texture.</returns>
    public Texture2D GetTexture(string key) => GetTexture(this[key]);

    /// <summary>Adds a frame on atlas page zero.</summary>
    /// <param name="key">The unique frame key.</param>
    /// <param name="cell">The atlas cell coordinate.</param>
    public void Add(string key, Coordinate2 cell) => Add(key, cell, 0);

    /// <summary>Adds a frame on the specified atlas page.</summary>
    /// <param name="key">The unique frame key.</param>
    /// <param name="cell">The atlas cell coordinate.</param>
    /// <param name="textureIndex">The zero-based index into <see cref="Textures"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The texture index is not registered.</exception>
    public void Add(string key, Coordinate2 cell, int textureIndex)
    {
        FrameSetTextures.ValidateIndex(Textures, textureIndex);
        _frames.Add(key, new TileFrameData(TileSize, cell, textureIndex));
    }

    /// <summary>Returns an enumerator over the registered frames.</summary>
    /// <returns>The frame enumerator.</returns>
    public IEnumerator<TileFrameData> GetEnumerator() => _frames.Values.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}