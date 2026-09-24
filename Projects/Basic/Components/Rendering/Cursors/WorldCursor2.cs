using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Graphics.Cameras;
using Sachssoft.Engine.Graphics.Meshes;
using Sachssoft.Engine.Graphics.Rendering;
using Sachssoft.Engine.Input;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Represents a cursor positioned and rendered in a two-dimensional world.
/// </summary>
public class WorldCursor2 : CursorComponent
{
    private IMesh? _cursorMesh;
    private WorldCursor2State _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldCursor2"/> class.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the cursor resources.
    /// </param>
    public WorldCursor2(GraphicsDevice graphicsDevice)
        : base(graphicsDevice)
    {
        _cursorMesh = MeshGenerator.CreateQuad(
            graphicsDevice,
            size: 1f,
            centerOrigin: false,
            flipMode: Graphics.Texture2DFlipMode.Vertical);
    }

    /// <summary>
    /// Gets or sets the rendering layer of the cursor.
    /// </summary>
    public float Layer { get; set; }

    /// <summary>
    /// Gets or sets the additional local transformation applied to the cursor.
    /// </summary>
    /// <remarks>
    /// The transformation is applied after the unit cursor mesh has been scaled
    /// to the dimensions of its texture. The cursor world position and runtime
    /// offset are applied separately.
    /// </remarks>
    public QuadTransform Transform { get; set; } = QuadTransform.Identity;

    /// <summary>
    /// Gets the current runtime state of the cursor.
    /// </summary>
    public WorldCursor2State State => _state;

    /// <summary>
    /// Applies the specified runtime state to the cursor.
    /// </summary>
    /// <param name="state">The runtime state to apply.</param>
    public void ApplyState(WorldCursor2State state)
    {
        _state = state;
    }

    /// <summary>
    /// Applies the specified cursor state using the given camera and offset.
    /// </summary>
    /// <param name="cursorState">
    /// The cursor state used to determine the world position.
    /// </param>
    /// <param name="camera">
    /// The camera used to transform the cursor position into world coordinates.
    /// </param>
    /// <param name="offset">
    /// The offset applied to the cursor representation.
    /// </param>
    public void ApplyState(
        ICursorState cursorState,
        ICamera camera,
        Vector2 offset)
    {
        _state = new WorldCursor2State(
            cursorState.GetWorldPosition(camera),
            offset);
    }

    /// <inheritdoc/>
    protected override void OnUnload()
    {
        _cursorMesh?.Dispose();
        _cursorMesh = null;

        base.OnUnload();
    }

    /// <inheritdoc/>
    public override void Draw(SceneDrawContext context)
    {
        if (!IsVisible || _cursorMesh == null)
            return;

        var shader = context.DefaultMaterial.Shader;

        shader.Texture = Texture;
        shader.Color = DiffuseColor;
        shader.Opacity = Opacity;
        shader.Apply();

        var textureScale = Texture != null
            ? new Vector3(Texture.Bounds.Width, Texture.Bounds.Height, 1f)
            : Vector3.One;

        var transform =
            Matrix.CreateScale(textureScale) *
            Transform.ToMatrix() *
            Matrix.CreateTranslation(
                _state.Position.X + _state.Offset.X,
                _state.Position.Y + _state.Offset.Y,
                Layer);

        MeshRenderer.Draw(
            context,
            _cursorMesh,
            shader: shader,
            transform: transform);
    }
}