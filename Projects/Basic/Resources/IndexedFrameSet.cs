using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Engine.Resources;

/// <summary>
/// Represents a texture frame set where frames are accessed through integer indices.
/// </summary>
public sealed class IndexedFrameSet : IFrameSet
{
    private readonly Dictionary<int, FrameData> _frames = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexedFrameSet"/> class.
    /// </summary>
    /// <param name="texture">
    /// Texture containing the frame data.
    /// </param>
    public IndexedFrameSet(Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);

        Texture = texture;
    }

    /// <summary>
    /// Gets the texture containing the frames.
    /// </summary>
    public Texture2D Texture { get; }

    /// <summary>
    /// Gets all indices of the registered frames.
    /// </summary>
    public IEnumerable<int> Indices => _frames.Keys;

    IEnumerable<object> IFrameSet.Keys => _frames.Keys.Cast<object>();

    /// <summary>
    /// Gets the frame associated with the specified index.
    /// </summary>
    public FrameData this[int index] => _frames[index];

    FrameData IFrameSet.this[object key] => this[(int)key];

    /// <summary>
    /// Adds a new frame using an integer index and atlas position.
    /// </summary>
    /// <param name="index">
    /// Integer value used to identify the frame.
    /// </param>
    /// <param name="position">
    /// Position of the frame inside the texture atlas.
    /// </param>
    /// <param name="size">
    /// Pixel size of the frame.
    /// </param>
    public void Add(
        int index,
        PixelPoint2 position,
        PixelSize2 size)
    {
        _frames.Add(
            index,
            new FrameData(position, size));
    }

    /// <summary>
    /// Gets the frame associated with the specified enum value.
    /// </summary>
    /// <typeparam name="TEnum">
    /// The enum type used as the frame index.
    /// </typeparam>
    /// <param name="index">
    /// The enum value identifying the frame.
    /// </param>
    /// <returns>
    /// The associated frame data.
    /// </returns>
    public FrameData Get<TEnum>(
        TEnum index)
        where TEnum : struct, Enum
    {
        return _frames[Convert.ToInt32(index)];
    }

    /// <summary>
    /// Determines whether a frame exists for the specified enum value.
    /// </summary>
    public bool Contains<TEnum>(
        TEnum index)
        where TEnum : struct, Enum
    {
        return _frames.ContainsKey(
            Convert.ToInt32(index));
    }

    /// <summary>
    /// Creates a strongly typed enum-indexed frame set from this frame set.
    /// </summary>
    /// <typeparam name="TEnum">
    /// The enum type used as the frame key.
    /// </typeparam>
    /// <returns>
    /// A strongly typed indexed frame set containing the same frames.
    /// </returns>
    public IndexedFrameSet<TEnum> ToEnumIndexed<TEnum>()
        where TEnum : struct, Enum
    {
        IndexedFrameSet<TEnum> result =
            new(Texture);

        foreach ((int index, FrameData frame) in _frames)
        {
            TEnum enumIndex =
                (TEnum)Enum.ToObject(
                    typeof(TEnum),
                    index);

            result.Add(
                enumIndex,
                frame.Location,
                frame.Size);
        }

        return result;
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