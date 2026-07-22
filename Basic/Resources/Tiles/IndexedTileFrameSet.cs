using Sachssoft.Sasogine.Assets.Graphics;
using Sachssoft.Sasogine.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Represents a texture frame set where frames are accessed through a strongly typed enum key.
///
/// This implementation is intended for runtime usage where frame references are known at compile time.
/// Using an enum avoids string-based lookups and provides type-safe access to tile frames.
/// </summary>
/// <typeparam name="TEnum">
/// Enum type used as the frame key.
/// </typeparam>
public sealed class IndexedTileFrameSet<TEnum> : ITileFrameSet
    where TEnum : struct, Enum
{
    private readonly Dictionary<TEnum, TileFrame> _frames = new();


    /// <summary>
    /// Creates a new indexed tile frame set.
    /// </summary>
    /// <param name="tileSize">
    /// Size of a single tile in pixels.
    /// </param>
    /// <param name="asset">
    /// Texture asset containing the tile frames.
    /// </param>
    public IndexedTileFrameSet(
        PixelSize tileSize,
        Texture2DAsset asset)
    {
        TileSize = tileSize;
        Asset = asset;
    }


    /// <summary>
    /// Gets the tile size used by this frame set.
    /// </summary>
    public PixelSize TileSize { get; }


    /// <summary>
    /// Gets the texture asset containing the frames.
    /// </summary>
    public Texture2DAsset Asset { get; }

    /// <summary>
    /// Gets all enum keys of the registered tile frames.
    /// </summary>
    public IEnumerable<TEnum> Indices => _frames.Keys;

    IEnumerable<object> ITileFrameSet.Keys
        => _frames.Keys.Cast<object>();

    TileFrame ITileFrameSet.this[object key]
        => this[(TEnum)key];

    /// <summary>
    /// Gets the frame associated with the specified enum index.
    /// </summary>
    public TileFrame this[TEnum index]
        => _frames[index];


    /// <summary>
    /// Adds a new frame using an enum index and atlas cell coordinate.
    /// </summary>
    /// <param name="index">
    /// Enum value used to identify the frame.
    /// </param>
    /// <param name="cell">
    /// Atlas cell coordinate of the frame.
    /// </param>
    public void Add(
        TEnum index,
        Coordinate2 cell)
    {
        _frames.Add(
            index,
            new TileFrame(TileSize, cell));
    }


    /// <summary>
    /// Returns an enumerator that iterates through all tile frames.
    /// </summary>
    public IEnumerator<TileFrame> GetEnumerator()
    {
        return _frames.Values.GetEnumerator();
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}