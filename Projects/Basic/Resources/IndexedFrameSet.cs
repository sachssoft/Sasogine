using Microsoft.Xna.Framework;
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
/// Using an enum avoids string-based lookups and provides type-safe access to frames.
/// </summary>
/// <typeparam name="TEnum">
/// Enum type used as the frame key.
/// </typeparam>
public sealed class IndexedFrameSet<TEnum> : IFrameSet
    where TEnum : struct, Enum
{
    private readonly Dictionary<TEnum, FrameData> _frames = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexedFrameSet{TEnum}"/> class.
    /// </summary>
    /// <param name="asset">
    /// Texture asset containing the frame data.
    /// </param>
    public IndexedFrameSet(
        Texture2DAsset asset)
    {
        Asset = asset;
    }

    /// <summary>
    /// Gets the texture asset containing the frames.
    /// </summary>
    public Texture2DAsset Asset { get; }

    /// <summary>
    /// Gets all enum keys of the registered frames.
    /// </summary>
    public IEnumerable<TEnum> Indices => _frames.Keys;

    IEnumerable<object> IFrameSet.Keys => _frames.Keys.Cast<object>();

    /// <summary>
    /// Gets the frame associated with the specified enum index.
    /// </summary>
    public FrameData this[TEnum index] => _frames[index];

    FrameData IFrameSet.this[object key] => this[(TEnum)key];

    /// <summary>
    /// Adds a new frame using an enum index and atlas position.
    /// </summary>
    /// <param name="index">
    /// Enum value used to identify the frame.
    /// </param>
    /// <param name="position">
    /// Position of the frame inside the texture atlas.
    /// </param>
    /// <param name="size">
    /// Pixel size of the frame.
    /// </param>
    public void Add(
        TEnum index,
        Point position,
        PixelSize2 size)
    {
        _frames.Add(
            index,
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