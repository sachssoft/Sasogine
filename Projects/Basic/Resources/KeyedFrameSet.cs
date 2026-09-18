using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Represents a frame set where frames are accessed using string keys.
///
/// This implementation is mainly intended for runtime usage where readable
/// frame names are useful.
/// </summary>
public sealed class KeyedFrameSet : IFrameSet
{
    private readonly Dictionary<string, FrameData> _frames = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyedFrameSet"/> class.
    /// </summary>
    /// <param name="texture">
    /// Texture containing the frame data.
    /// </param>
    public KeyedFrameSet(Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);

        Texture = texture;
    }

    /// <summary>
    /// Gets the texture containing the frames.
    /// </summary>
    public Texture2D Texture { get; }

    /// <summary>
    /// Gets all string keys used to identify the registered frames.
    /// </summary>
    public IEnumerable<string> Keys
        => _frames.Keys;

    IEnumerable<object> IFrameSet.Keys
        => _frames.Keys;

    /// <summary>
    /// Gets the frame associated with the specified key.
    /// </summary>
    /// <param name="key">
    /// String key of the frame.
    /// </param>
    public FrameData this[string key]
        => _frames[key];

    FrameData IFrameSet.this[object key]
        => this[(string)key];

    /// <summary>
    /// Adds a frame using a string key and atlas position.
    /// </summary>
    /// <param name="key">
    /// Key used to identify the frame.
    /// </param>
    /// <param name="position">
    /// Position of the frame inside the texture atlas.
    /// </param>
    /// <param name="size">
    /// Pixel size of the frame.
    /// </param>
    public void Add(
        string key,
        Point position,
        PixelSize2 size)
    {
        _frames.Add(
            key,
            new FrameData(position, size));
    }

    /// <summary>
    /// Returns an enumerator that iterates through all frames.
    /// </summary>
    public IEnumerator<FrameData> GetEnumerator()
    {
        return _frames.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}