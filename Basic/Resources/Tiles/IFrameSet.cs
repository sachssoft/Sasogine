using Sachssoft.Sasogine.Assets.Graphics;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Defines a collection of tile frames that belong to a texture asset.
///
/// A tile frame set provides access to registered tile frames using an
/// implementation-specific key type and supports enumeration of all contained
/// frames.
///
/// Implementations may provide different lookup strategies, such as sequential
/// indices, enum-based indices, or string-based keys.
/// </summary>
public interface ITileFrameSet : IEnumerable<TileFrame>
{
    /// <summary>
    /// Gets the texture asset containing the tile frames.
    /// </summary>
    Texture2DAsset Asset { get; }

    /// <summary>
    /// Gets a tile frame using an implementation-specific key.
    /// </summary>
    /// <param name="key">
    /// Key used to identify the tile frame.
    /// </param>
    /// <returns>
    /// The tile frame associated with the specified key.
    /// </returns>
    TileFrame this[object key] { get; }

    /// <summary>
    /// Gets all keys used to identify the registered tile frames.
    /// </summary>
    IEnumerable<object> Keys { get; }
}