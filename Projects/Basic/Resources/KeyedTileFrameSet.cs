using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Engine.Resources;

/// <summary>
/// Represents a tile frame set where frames are accessed using string keys.
///
/// This implementation is intended for runtime usage where readable
/// frame names are useful.
/// </summary>
public sealed class KeyedTileFrameSet : ITileFrameSet
{
    private readonly Dictionary<string, TileFrameData> _frames = new();

    /// <summary>
    /// Creates a new keyed tile frame set.
    /// </summary>
    /// <param name="tileSize">
    /// Size of a single tile in pixels.
    /// </param>
    /// <param name="texture">
    /// Texture containing the tile frames.
    /// </param>
    public KeyedTileFrameSet(
        PixelSize2 tileSize,
        Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);

        TileSize = tileSize;
        Texture = texture;
    }

    /// <summary>
    /// Gets the tile size used by this frame set.
    /// </summary>
    public PixelSize2 TileSize { get; }

    /// <summary>
    /// Gets the texture containing the tile frames.
    /// </summary>
    public Texture2D Texture { get; }

    /// <summary>
    /// Gets all string keys used to identify the registered tile frames.
    /// </summary>
    public IEnumerable<string> Keys
        => _frames.Keys;

    IEnumerable<object> ITileFrameSet.Keys
        => _frames.Keys;

    /// <summary>
    /// Gets the frame associated with the specified key.
    /// </summary>
    /// <param name="key">
    /// String key of the frame.
    /// </param>
    public TileFrameData this[string key]
        => _frames[key];

    TileFrameData ITileFrameSet.this[object key]
        => this[(string)key];

    /// <summary>
    /// Adds a frame using a string key and atlas cell coordinate.
    /// </summary>
    /// <param name="key">
    /// Key used to identify the frame.
    /// </param>
    /// <param name="cell">
    /// Atlas cell coordinate of the frame.
    /// </param>
    public void Add(
        string key,
        Coordinate2 cell)
    {
        _frames.Add(
            key,
            new TileFrameData(TileSize, cell));
    }

    /// <summary>
    /// Returns an enumerator that iterates through all tile frames.
    /// </summary>
    public IEnumerator<TileFrameData> GetEnumerator()
    {
        return _frames.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}