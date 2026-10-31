using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Engine.Resources;

/// <summary>Stores frames across one or more borrowed atlas textures.</summary>
/// <remarks>Textures are not disposed by this collection. Use each frame's texture index when rendering.</remarks>
public sealed class KeyedFrameSet : IFrameSet
{
    /// <summary>Stores frames by their strongly typed key.</summary>
    private readonly Dictionary<string, FrameData> _frames = new();

    /// <summary>Creates a frame set backed by one atlas texture.</summary>
    /// <param name="texture">The borrowed texture containing page zero.</param>
    public KeyedFrameSet(Texture2D texture)
    {
        Textures = FrameSetTextures.Create(texture);
    }

    /// <summary>Creates a frame set backed by multiple ordered atlas textures.</summary>
    /// <param name="textures">The nonempty, ordered atlas pages. Their references are copied.</param>
    public KeyedFrameSet(IReadOnlyList<Texture2D> textures)
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
    public IEnumerable<string> Keys => _frames.Keys;

    /// <inheritdoc/>
    IEnumerable<object> IFrameSet.Keys => _frames.Keys;

    /// <summary>Gets the frame associated with the specified key.</summary>
    /// <param name="key">The registered frame key.</param>
    /// <returns>The frame, including its atlas page index.</returns>
    public FrameData this[string key] => _frames[key];

    /// <inheritdoc/>
    FrameData IFrameSet.this[object key] => this[(string)key];

    /// <summary>Gets the atlas texture referenced by a frame.</summary>
    /// <param name="frame">The frame whose page should be resolved.</param>
    /// <returns>The corresponding borrowed texture.</returns>
    public Texture2D GetTexture(FrameData frame)
    {
        FrameSetTextures.ValidateIndex(Textures, frame.TextureIndex);
        return Textures[frame.TextureIndex];
    }

    /// <summary>Gets the atlas texture for a registered frame key.</summary>
    /// <param name="key">The frame key.</param>
    /// <returns>The corresponding borrowed texture.</returns>
    public Texture2D GetTexture(string key) => GetTexture(this[key]);

    /// <summary>Adds a frame on atlas page zero.</summary>
    /// <param name="key">The unique frame key.</param>
    /// <param name="position">The frame location in pixels.</param>
    /// <param name="size">The frame size in pixels.</param>
    public void Add(string key, Point position, PixelSize2 size) => Add(key, position, size, 0);

    /// <summary>Adds a frame on the specified atlas page.</summary>
    /// <param name="key">The unique frame key.</param>
    /// <param name="position">The frame location in pixels.</param>
    /// <param name="size">The frame size in pixels.</param>
    /// <param name="textureIndex">The zero-based index into <see cref="Textures"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The texture index is not registered.</exception>
    public void Add(string key, Point position, PixelSize2 size, int textureIndex)
    {
        FrameSetTextures.ValidateIndex(Textures, textureIndex);
        _frames.Add(key, new FrameData(position, size, textureIndex));
    }

    /// <summary>Adds an existing frame without discarding its atlas page.</summary>
    /// <param name="key">The unique frame key.</param>
    /// <param name="frame">The complete frame data.</param>
    public void Add(string key, FrameData frame)
    {
        FrameSetTextures.ValidateIndex(Textures, frame.TextureIndex);
        _frames.Add(key, frame);
    }

    /// <summary>Returns an enumerator over the registered frames.</summary>
    /// <returns>The frame enumerator.</returns>
    public IEnumerator<FrameData> GetEnumerator() => _frames.Values.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}