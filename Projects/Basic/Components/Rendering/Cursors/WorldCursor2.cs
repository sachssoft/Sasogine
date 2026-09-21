using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Meshes;
using Sachssoft.Sasogine.Graphics.Rendering;
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
    public void ApplyState(WorldCursor2State state)
    {
        _state = state;
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
            _state.Position.X,
            _state.Position.Y,
            Layer);

        MeshRenderer.Draw(
            context,
            _cursorMesh,
            shader: shader,
            transform: transform);
    }
}