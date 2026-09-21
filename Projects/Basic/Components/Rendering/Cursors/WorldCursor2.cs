using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Meshes;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Input;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering;

/// <summary>
/// Represents a cursor positioned in a two-dimensional world.
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
            centerOrigin: false);
    }

    /// <summary>
    /// Gets or sets the rendering layer of the cursor.
    /// </summary>
    public float Layer { get; set; }

    /// <summary>
    /// Gets the current runtime state of the cursor.
    /// </summary>
    public WorldCursor2State State => _state;

    /// <summary>
    /// Applies the specified runtime state to the cursor.
    /// </summary>
    /// <param name="state">
    /// The runtime state to apply.
    /// </param>
    public void ApplyState(
        WorldCursor2State state)
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

        var transform = Matrix.CreateTranslation(
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