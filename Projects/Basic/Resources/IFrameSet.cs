using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Sachssoft.Engine.Resources;

/// <summary>
/// Defines a collection of frames that reference one or more atlas textures.
///
/// A frame set provides access to registered frames using an
/// implementation-specific key type and supports enumeration of all contained
/// frames.
///
/// Implementations may provide different lookup strategies, such as sequential
/// indices, enum-based indices, or string-based keys.
/// </summary>
public interface IFrameSet : IEnumerable<FrameData>
{
    /// <summary>
    /// Gets atlas page zero for compatibility with single-texture frame sets.
    /// </summary>
    Texture2D Texture
    {
        get;
    }

    /// <summary>Gets the ordered atlas pages borrowed by this frame set.</summary>
    IReadOnlyList<Texture2D> Textures
    {
        get;
    }

    /// <summary>Resolves the atlas page referenced by a frame.</summary>
    /// <param name="frame">The frame whose texture should be resolved.</param>
    /// <returns>The corresponding borrowed texture.</returns>
    Texture2D GetTexture(FrameData frame);

    /// <summary>
    /// Gets a frame using an implementation-specific key.
    /// </summary>
    /// <param name="key">
    /// Key used to identify the frame.
    /// </param>
    /// <returns>
    /// The frame associated with the specified key.
    /// </returns>
    FrameData this[object key]
    {
        get;
    }

    /// <summary>
    /// Gets all keys used to identify the registered frames.
    /// </summary>
    IEnumerable<object> Keys
    {
        get;
    }
}
