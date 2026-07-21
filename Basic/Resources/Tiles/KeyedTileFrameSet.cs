using Sachssoft.Sasogine.Assets.Graphics;
using Sachssoft.Sasogine.Common;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Represents a tile frame set where frames are accessed using string keys.
///
/// This implementation is mainly intended for asset loading, editor usage,
/// and content definitions where readable frame names are useful.
/// </summary>
public sealed class KeyedTileFrameSet : ITileFrameSet
{
    private readonly Dictionary<string, TileFrame> _frames = new();


    /// <summary>
    /// Creates a new keyed tile frame set.
    /// </summary>
    /// <param name="tileSize">
    /// Size of a single tile in pixels.
    /// </param>
    /// <param name="asset">
    /// Texture asset containing the tile frames.
    /// </param>
    public KeyedTileFrameSet(
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
    /// Gets all registered tile frames.
    /// </summary>
    public IEnumerable<TileFrame> Frames
        => _frames.Values;


    TileFrame ITileFrameSet.this[object key]
        => this[(string)key];


    /// <summary>
    /// Gets the frame associated with the specified key.
    /// </summary>
    /// <param name="key">
    /// String key of the frame.
    /// </param>
    public TileFrame this[string key]
        => _frames[key];


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