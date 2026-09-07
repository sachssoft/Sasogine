using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Rendering.Batches;

/// <summary>
/// Batches frames from multiple textures by grouping them by texture.
/// </summary>
/// <remarks>
/// <para>
/// Frames using the same texture are rendered together through an internal
/// <see cref="FrameBatch"/>, reducing texture changes and draw calls.
/// </para>
/// <para>
/// Frame order is preserved within each texture group, but not between
/// different textures.
/// </para>
/// </remarks>
public sealed class MultiFrameBatch : IDisposable
{
    private readonly FrameBatch _frameBatch;
    private readonly Dictionary<Texture2D, int> _groupLookup = [];
    private readonly List<FrameGroup> _groups = [];

    private IShader? _shader;
    private ICamera? _camera;
    private int _groupCount;
    private bool _isBegun;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiFrameBatch"/> class.
    /// </summary>
    /// <param name="graphicsDevice">
    /// Graphics device used for rendering.
    /// </param>
    /// <param name="initialCapacity">
    /// Initial capacity of the underlying frame batch.
    /// </param>
    public MultiFrameBatch(
        GraphicsDevice graphicsDevice,
        int initialCapacity = 1024)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        _frameBatch = new FrameBatch(
            graphicsDevice,
            initialCapacity);
    }

    /// <summary>
    /// Starts collecting frames.
    /// </summary>
    /// <param name="shader">
    /// Shader used for rendering.
    /// </param>
    /// <param name="camera">
    /// Camera used for rendering.
    /// </param>
    public void Begin(
        IShader shader,
        ICamera camera)
    {
        ThrowIfDisposed();

        if (_isBegun)
        {
            throw new InvalidOperationException(
                "Batch is already active. Call End() before Begin().");
        }

        ArgumentNullException.ThrowIfNull(shader);
        ArgumentNullException.ThrowIfNull(camera);

        _shader = shader;
        _camera = camera;

        _groupLookup.Clear();
        _groupCount = 0;

        _isBegun = true;
    }

    /// <summary>
    /// Adds a frame at a world position.
    /// </summary>
    /// <param name="texture">
    /// Texture containing the frame.
    /// </param>
    /// <param name="position">
    /// World position of the frame.
    /// </param>
    /// <param name="sourceBounds">
    /// Pixel bounds inside the texture.
    /// </param>
    /// <param name="color">
    /// Color tint.
    /// </param>
    public void AddFrame(
        Texture2D texture,
        Point2 position,
        PixelBounds2 sourceBounds,
        Color color)
    {
        QuadTransform transform = new()
        {
            Position = position,
            Scale = Vector2.One,
            Rotation = 0f,
            Origin = Vector2.Zero
        };

        AddFrame(
            texture,
            transform,
            sourceBounds,
            color);
    }

    /// <summary>
    /// Adds a transformed frame.
    /// </summary>
    /// <param name="texture">
    /// Texture containing the frame.
    /// </param>
    /// <param name="transform">
    /// Transformation applied to the frame.
    /// </param>
    /// <param name="sourceBounds">
    /// Pixel bounds inside the texture.
    /// </param>
    /// <param name="color">
    /// Color tint.
    /// </param>
    public void AddFrame(
        Texture2D texture,
        QuadTransform transform,
        PixelBounds2 sourceBounds,
        Color color)
    {
        ThrowIfDisposed();
        ThrowIfNotBegun();

        ArgumentNullException.ThrowIfNull(texture);

        FrameGroup group = GetGroup(texture);

        group.Frames.Add(
            new FrameEntry(
                transform,
                sourceBounds,
                color));
    }

    /// <summary>
    /// Renders all collected frames grouped by texture.
    /// </summary>
    public void End()
    {
        ThrowIfDisposed();
        ThrowIfNotBegun();

        try
        {
            IShader shader = _shader!;
            ICamera camera = _camera!;

            for (int i = 0; i < _groupCount; i++)
            {
                FrameGroup group = _groups[i];
                Texture2D texture = group.Texture!;

                _frameBatch.Begin(
                    shader,
                    camera,
                    texture);

                List<FrameEntry> frames = group.Frames;

                for (int j = 0; j < frames.Count; j++)
                {
                    FrameEntry frame = frames[j];

                    _frameBatch.AddFrame(
                        frame.Transform,
                        frame.SourceBounds,
                        frame.Color);
                }

                _frameBatch.End();
            }
        }
        finally
        {
            Clear();
            _isBegun = false;
        }
    }

    /// <summary>
    /// Releases graphics resources used by this batch.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _frameBatch.Dispose();

        _groups.Clear();
        _groupLookup.Clear();

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    private FrameGroup GetGroup(Texture2D texture)
    {
        if (_groupLookup.TryGetValue(
            texture,
            out int groupIndex))
        {
            return _groups[groupIndex];
        }

        groupIndex = _groupCount++;

        FrameGroup group;

        if (groupIndex < _groups.Count)
        {
            group = _groups[groupIndex];
            group.Frames.Clear();
        }
        else
        {
            group = new FrameGroup();
            _groups.Add(group);
        }

        group.Texture = texture;

        _groupLookup.Add(
            texture,
            groupIndex);

        return group;
    }

    private void Clear()
    {
        _groupLookup.Clear();

        for (int i = 0; i < _groupCount; i++)
        {
            FrameGroup group = _groups[i];

            group.Texture = null;
            group.Frames.Clear();
        }

        _groupCount = 0;
        _shader = null;
        _camera = null;
    }

    private void ThrowIfNotBegun()
    {
        if (!_isBegun)
        {
            throw new InvalidOperationException(
                "Batch has not been started. Call Begin() first.");
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private sealed class FrameGroup
    {
        public Texture2D? Texture;

        public readonly List<FrameEntry> Frames = [];
    }

    private readonly struct FrameEntry
    {
        public FrameEntry(
            QuadTransform transform,
            PixelBounds2 sourceBounds,
            Color color)
        {
            Transform = transform;
            SourceBounds = sourceBounds;
            Color = color;
        }

        public QuadTransform Transform { get; }

        public PixelBounds2 SourceBounds { get; }

        public Color Color { get; }
    }
}