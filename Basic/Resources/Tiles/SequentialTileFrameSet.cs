using Sachssoft.Sasogine.Assets.Graphics;
using Sachssoft.Sasogine.Common;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Represents a tile frame set where frames are stored and accessed in sequential order.
///
/// This implementation is intended for cases where frame indices are determined by
/// insertion order. Frames are stored in a list and can be accessed using a zero-based
/// integer index.
///
/// This provides fast indexed access without requiring string keys or additional
/// lookup structures.
/// </summary>
public sealed class SequentialTileFrameSet : ITileFrameSet
{
    private readonly List<TileFrame> _frames = new();


    /// <summary>
    /// Creates a new sequential tile frame set.
    /// </summary>
    /// <param name="tileSize">
    /// Size of a single tile in pixels.
    /// </param>
    /// <param name="asset">
    /// Texture asset containing the tile frames.
    /// </param>
    public SequentialTileFrameSet(
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
    /// Gets the number of tile frames contained in this frame set.
    /// </summary>
    public int Count => _frames.Count;

    TileFrame ITileFrameSet.this[object key]
        => this[(int)key];


    /// <summary>
    /// Gets the frame at the specified zero-based index.
    /// </summary>
    /// <param name="index">
    /// Zero-based index of the frame.
    /// </param>
    public TileFrame this[int index]
        => _frames[index];


    /// <summary>
    /// Adds a new frame to the end of the frame sequence.
    ///
    /// The assigned index is determined automatically by the insertion order.
    /// </summary>
    /// <param name="cell">
    /// Atlas cell coordinate of the frame.
    /// </param>
    public void Add(
        Coordinate2 cell)
    {
        _frames.Add(
            new TileFrame(TileSize, cell));
    }

    /// <summary>
    /// Returns an enumerator that iterates through all tile frames in insertion order.
    /// </summary>
    public IEnumerator<TileFrame> GetEnumerator()
    {
        return _frames.GetEnumerator();
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}