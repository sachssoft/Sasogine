using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Sachssoft.Engine.Resources;

/// <summary>
/// Defines a collection of tile frames that reference one or more atlas textures.
///
/// A tile frame set provides access to registered tile frames using an
/// implementation-specific key type and supports enumeration of all contained
/// frames.
///
/// Implementations may provide different lookup strategies, such as sequential
/// indices, enum-based indices, or string-based keys.
/// </summary>
public interface ITileFrameSet : IEnumerable<TileFrameData>
{
    /// <summary>
    /// Gets atlas page zero for compatibility with single-texture frame sets.
    /// </summary>
    Texture2D Texture
    {
        get;
    }

    /// <summary>Gets the ordered atlas pages borrowed by this tile frame set.</summary>
    IReadOnlyList<Texture2D> Textures
    {
        get;
    }

    /// <summary>Resolves the atlas page referenced by a tile frame.</summary>
    /// <param name="frame">The tile frame whose texture should be resolved.</param>
    /// <returns>The corresponding borrowed texture.</returns>
    Texture2D GetTexture(TileFrameData frame);

    /// <summary>
    /// Gets a tile frame using an implementation-specific key.
    /// </summary>
    /// <param name="key">
    /// Key used to identify the tile frame.
    /// </param>
    /// <returns>
    /// The tile frame associated with the specified key.
    /// </returns>
    TileFrameData this[object key]
    {
        get;
    }

    /// <summary>
    /// Gets all keys used to identify the registered tile frames.
    /// </summary>
    IEnumerable<object> Keys
    {
        get;
    }
}
