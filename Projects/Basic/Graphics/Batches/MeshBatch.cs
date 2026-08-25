using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Meshes;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Rendering.Batches;

/// <summary>
/// Provides batched rendering for GPU meshes using a shared shader and camera.
/// </summary>
/// <remarks>
/// Meshes are collected between <see cref="Begin"/> and <see cref="End"/>.
/// Each mesh may use its own transformation matrix while sharing the same
/// shader, camera and optional texture.
/// </remarks>
public sealed class MeshBatch : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly List<MeshEntry> _entries;

    private bool _isBegun;
    private bool _disposed;

    private IShader? _shader;
    private ICamera? _camera;
    private Texture2D? _texture;

    private readonly struct MeshEntry
    {
        public readonly IMesh Mesh;
        public readonly Matrix Transform;

        public MeshEntry(IMesh mesh, Matrix transform)
        {
            Mesh = mesh;
            Transform = transform;
        }
    }

    /// <summary>
    /// Creates a new mesh batch.
    /// </summary>
    /// <param name="graphicsDevice">
    /// Graphics device used for rendering.
    /// </param>
    /// <param name="initialCapacity">
    /// Initial mesh capacity.
    /// </param>
    public MeshBatch(
        GraphicsDevice graphicsDevice,
        int initialCapacity = 64)
    {
        _graphicsDevice =
            graphicsDevice ??
            throw new ArgumentNullException(nameof(graphicsDevice));

        if (initialCapacity < 1)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));

        _entries = new List<MeshEntry>(initialCapacity);
    }

    /// <summary>
    /// Gets the number of meshes currently accumulated in the batch.
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Starts rendering.
    /// </summary>
    /// <param name="shader">
    /// Shader used for rendering.
    /// </param>
    /// <param name="camera">
    /// Camera used for rendering.
    /// </param>
    /// <param name="texture">
    /// Optional texture used by the shader.
    /// </param>
    public void Begin(
        IShader shader,
        ICamera camera,
        Texture2D? texture = null)
    {
        ThrowIfDisposed();

        if (_isBegun)
            throw new InvalidOperationException(
                "Batch is already active. Call End() before Begin().");

        _shader =
            shader ??
            throw new ArgumentNullException(nameof(shader));

        _camera =
            camera ??
            throw new ArgumentNullException(nameof(camera));

        _texture = texture;

        _entries.Clear();
        _isBegun = true;
    }

    /// <summary>
    /// Adds a mesh using the identity transformation.
    /// </summary>
    /// <param name="mesh">
    /// Mesh to add.
    /// </param>
    public void Add(IMesh mesh)
    {
        Add(mesh, Matrix.Identity);
    }

    /// <summary>
    /// Adds a mesh using the specified transformation.
    /// </summary>
    /// <param name="mesh">
    /// Mesh to add.
    /// </param>
    /// <param name="transform">
    /// Transformation applied to the mesh.
    /// </param>
    public void Add(
        IMesh mesh,
        Matrix transform)
    {
        ThrowIfDisposed();

        if (!_isBegun)
            throw new InvalidOperationException(
                "Batch must be started with Begin() before adding meshes.");

        ArgumentNullException.ThrowIfNull(mesh);

        _entries.Add(
            new MeshEntry(
                mesh,
                transform));
    }

    /// <summary>
    /// Adds a typed mesh using the identity transformation.
    /// </summary>
    /// <typeparam name="TVertex">
    /// Vertex type stored by the mesh.
    /// </typeparam>
    /// <param name="mesh">
    /// Mesh to add.
    /// </param>
    public void Add<TVertex>(
        Mesh<TVertex> mesh)
        where TVertex : struct, IVertexType
    {
        Add(
            mesh,
            Matrix.Identity);
    }

    /// <summary>
    /// Adds a typed mesh using the specified transformation.
    /// </summary>
    /// <typeparam name="TVertex">
    /// Vertex type stored by the mesh.
    /// </typeparam>
    /// <param name="mesh">
    /// Mesh to add.
    /// </param>
    /// <param name="transform">
    /// Transformation applied to the mesh.
    /// </param>
    public void Add<TVertex>(
        Mesh<TVertex> mesh,
        Matrix transform)
        where TVertex : struct, IVertexType
    {
        Add(
            (IMesh)mesh,
            transform);
    }

    /// <summary>
    /// Draws the accumulated meshes.
    /// </summary>
    public void End()
    {
        ThrowIfDisposed();

        if (!_isBegun)
            throw new InvalidOperationException(
                "Batch has not been started. Call Begin() first.");

        try
        {
            if (_shader == null ||
                _camera == null ||
                _entries.Count == 0)
                return;

            if (_shader is IShaderTransform shaderTransform)
                shaderTransform.Camera = _camera;

            _shader.Texture = _texture;
            _shader.Color = Color.White;

            foreach (var entry in _entries)
            {
                if (_shader is IShaderTransform transform)
                    transform.Transform = entry.Transform;

                _shader.Apply();

                _graphicsDevice.SetVertexBuffer(
                    entry.Mesh.VertexBuffer);

                _graphicsDevice.Indices =
                    entry.Mesh.IndexBuffer;

                foreach (var pass in _shader.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    _graphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        entry.Mesh.IndexCount / 3);
                }
            }
        }
        finally
        {
            _entries.Clear();

            _shader = null;
            _camera = null;
            _texture = null;

            _isBegun = false;
        }
    }

    /// <summary>
    /// Releases resources used by the mesh batch.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _entries.Clear();

        _shader = null;
        _camera = null;
        _texture = null;

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(
                nameof(MeshBatch));
    }
}