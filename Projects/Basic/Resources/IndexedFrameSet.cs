using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Engine.Resources;

/// <summary>Stores frames across one or more borrowed atlas textures.</summary>
/// <remarks>Textures are not disposed by this collection. Use each frame's texture index when rendering.</remarks>
public sealed class IndexedFrameSet : IFrameSet
{
    /// <summary>Stores frames by their strongly typed key.</summary>
    private readonly Dictionary<int, FrameData> _frames = new();

    /// <summary>Creates a frame set backed by one atlas texture.</summary>
    /// <param name="texture">The borrowed texture containing page zero.</param>
    public IndexedFrameSet(Texture2D texture)
    {
        Textures = FrameSetTextures.Create(texture);
    }

    /// <summary>Creates a frame set backed by multiple ordered atlas textures.</summary>
    /// <param name="textures">The nonempty, ordered atlas pages. Their references are copied.</param>
    public IndexedFrameSet(IReadOnlyList<Texture2D> textures)
    {
        Textures = FrameSetTextures.Create(textures);
    }

    /// <summary>Gets the ordered atlas pages without transferring texture ownership.</summary>
    public IReadOnlyList<Texture2D> Textures
    {
        get;
    }

    /// <summary>Gets page zero for compatibility with single-texture frame sets.</summary>
    /// <remarks>For multiple pages, use <see cref="GetTexture(FrameData)"/> or the frame's texture index.</remarks>
    public Texture2D Texture => Textures[0];

    /// <summary>Gets the registered frame keys.</summary>
    public IEnumerable<int> Indices => _frames.Keys;

    /// <inheritdoc/>
    IEnumerable<object> IFrameSet.Keys => _frames.Keys.Cast<object>();

    /// <summary>Gets the frame associated with the specified key.</summary>
    /// <param name="index">The registered frame key.</param>
    /// <returns>The frame, including its atlas page index.</returns>
    public FrameData this[int index] => _frames[index];

    /// <inheritdoc/>
    FrameData IFrameSet.this[object key] => this[(int)key];

    /// <summary>Gets the atlas texture referenced by a frame.</summary>
    /// <param name="frame">The frame whose page should be resolved.</param>
    /// <returns>The corresponding borrowed texture.</returns>
    public Texture2D GetTexture(FrameData frame)
    {
        FrameSetTextures.ValidateIndex(Textures, frame.TextureIndex);
        return Textures[frame.TextureIndex];
    }

    /// <summary>Gets the atlas texture for a registered frame key.</summary>
    /// <param name="index">The frame key.</param>
    /// <returns>The corresponding borrowed texture.</returns>
    public Texture2D GetTexture(int index) => GetTexture(this[index]);

    /// <summary>Adds a frame on atlas page zero.</summary>
    /// <param name="index">The unique frame key.</param>
    /// <param name="position">The frame location in pixels.</param>
    /// <param name="size">The frame size in pixels.</param>
    public void Add(int index, PixelPoint2 position, PixelSize2 size) => Add(index, position, size, 0);

    /// <summary>Adds a frame on the specified atlas page.</summary>
    /// <param name="index">The unique frame key.</param>
    /// <param name="position">The frame location in pixels.</param>
    /// <param name="size">The frame size in pixels.</param>
    /// <param name="textureIndex">The zero-based index into <see cref="Textures"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The texture index is not registered.</exception>
    public void Add(int index, PixelPoint2 position, PixelSize2 size, int textureIndex)
    {
        FrameSetTextures.ValidateIndex(Textures, textureIndex);
        _frames.Add(index, new FrameData(position, size, textureIndex));
    }

    /// <summary>Adds an existing frame without discarding its atlas page.</summary>
    /// <param name="index">The unique frame key.</param>
    /// <param name="frame">The complete frame data.</param>
    public void Add(int index, FrameData frame)
    {
        FrameSetTextures.ValidateIndex(Textures, frame.TextureIndex);
        _frames.Add(index, frame);
    }

    /// <summary>Gets a frame by converting its enum key to an integer.</summary>
    /// <typeparam name="TEnum">The enum key type.</typeparam>
    /// <param name="index">The enum key.</param>
    /// <returns>The corresponding frame.</returns>
    public FrameData Get<TEnum>(TEnum index) where TEnum : struct, Enum => _frames[Convert.ToInt32(index)];

    /// <summary>Checks whether an enum key has a registered frame.</summary>
    /// <typeparam name="TEnum">The enum key type.</typeparam>
    /// <param name="index">The enum key.</param>
    /// <returns>Whether a frame exists for the enum key.</returns>
    public bool Contains<TEnum>(TEnum index) where TEnum : struct, Enum => _frames.ContainsKey(Convert.ToInt32(index));

    /// <summary>Copies frames into an enum-indexed collection while preserving all atlas pages.</summary>
    /// <typeparam name="TEnum">The enum key type.</typeparam>
    /// <returns>A new frame collection borrowing the same textures.</returns>
    public IndexedFrameSet<TEnum> ToEnumIndexed<TEnum>() where TEnum : struct, Enum
    {
        var result = new IndexedFrameSet<TEnum>(Textures);
        foreach ((int index, FrameData frame) in _frames)
            result.Add((TEnum)Enum.ToObject(typeof(TEnum), index), frame);
        return result;
    }

    /// <summary>Returns an enumerator over the registered frames.</summary>
    /// <returns>The frame enumerator.</returns>
    public IEnumerator<FrameData> GetEnumerator() => _frames.Values.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}