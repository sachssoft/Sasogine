using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Meshes;
using Sachssoft.Engine.Graphics.Rendering;
using Sachssoft.Engine.Scenes;
using System;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Represents a cursor positioned and rendered in a three-dimensional world.
/// </summary>
public class WorldCursor3 : CursorComponent
{
    private IMesh? _cursorMesh;
    private bool _ownsMesh;
    private WorldCursor3State _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldCursor3"/> class
    /// using a default cursor mesh.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the default cursor mesh.
    /// </param>
    public WorldCursor3(GraphicsDevice graphicsDevice)
        : base(graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        _cursorMesh = MeshGenerator.CreateQuad(
            graphicsDevice,
            centerOrigin: true);

        _ownsMesh = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldCursor3"/> class
    /// using the specified cursor mesh.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device associated with the cursor.
    /// </param>
    /// <param name="mesh">
    /// The mesh used to render the cursor.
    /// </param>
    public WorldCursor3(
        GraphicsDevice graphicsDevice,
        IMesh mesh)
        : base(graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(mesh);

        _cursorMesh = mesh;
        _ownsMesh = false;
    }

    /// <summary>
    /// Gets the current runtime state of the cursor.
    /// </summary>
    public WorldCursor3State State => _state;

    /// <summary>
    /// Gets the mesh currently used to render the cursor.
    /// </summary>
    public IMesh? Mesh => _cursorMesh;

    /// <summary>
    /// Gets whether the cursor currently has a mesh.
    /// </summary>
    public bool HasMesh => _cursorMesh is not null;

    /// <summary>
    /// Gets whether the cursor owns the current mesh.
    /// </summary>
    public bool OwnsMesh => _ownsMesh;

    /// <summary>
    /// Applies the specified runtime state to the cursor.
    /// </summary>
    /// <param name="state">
    /// The runtime state to apply.
    /// </param>
    public void ApplyState(WorldCursor3State state)
    {
        _state = state;
    }

    /// <summary>
    /// Sets the mesh used to render the cursor.
    /// </summary>
    /// <param name="mesh">
    /// The mesh to use, or <see langword="null"/> to remove the current mesh.
    /// </param>
    /// <param name="takeOwnership">
    /// Whether the cursor owns and disposes the specified mesh.
    /// </param>
    public void SetMesh(
        IMesh? mesh,
        bool takeOwnership = false)
    {
        ReleaseOwnedMesh();

        _cursorMesh = mesh;
        _ownsMesh = mesh is not null && takeOwnership;
    }

    /// <summary>
    /// Removes the current mesh from the cursor.
    /// </summary>
    public void ClearMesh()
    {
        ReleaseOwnedMesh();

        _cursorMesh = null;
        _ownsMesh = false;
    }

    /// <inheritdoc/>
    protected override void OnUnload()
    {
        ReleaseOwnedMesh();

        _cursorMesh = null;
        _ownsMesh = false;

        base.OnUnload();
    }

    /// <inheritdoc/>
    public override void Draw(SceneDrawContext context)
    {
        if (!IsVisible ||
            !IsEnabled ||
            _cursorMesh is null)
        {
            return;
        }

        var shader = context.DefaultMaterial.Shader;

        shader.Texture = Texture;
        shader.Color = DiffuseColor;
        shader.Opacity = Opacity;
        shader.Apply();

        MeshRenderer.Draw(
            context,
            _cursorMesh,
            shader: shader,
            transform: _state.GetTransform());
    }

    private void ReleaseOwnedMesh()
    {
        if (_ownsMesh)
            _cursorMesh?.Dispose();
    }
}