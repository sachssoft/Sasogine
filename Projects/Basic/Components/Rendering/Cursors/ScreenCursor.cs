using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering;

/// <summary>
/// Represents a cursor positioned in screen coordinates.
/// </summary>
public class ScreenCursor : CursorComponent
{
    private readonly SpriteBatch _spriteBatch;
    private ScreenCursorState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenCursor"/> class.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the sprite batch.
    /// </param>
    public ScreenCursor(GraphicsDevice graphicsDevice)
        : base(graphicsDevice)
    {
        _spriteBatch = new SpriteBatch(graphicsDevice);
    }

    /// <summary>
    /// Gets or sets the rendering layer of the cursor.
    /// </summary>
    public float Layer { get; set; }

    /// <summary>
    /// Gets or sets the source region of the cursor texture.
    /// </summary>
    public ISourceRegion? TextureRegion { get; set; }

    /// <summary>
    /// Gets the current runtime state of the cursor.
    /// </summary>
    public ScreenCursorState State => _state;

    /// <summary>
    /// Applies the specified runtime state to the cursor.
    /// </summary>
    /// <param name="state">
    /// The runtime state to apply.
    /// </param>
    public void ApplyState(ScreenCursorState state)
    {
        _state = state;
    }

    /// <inheritdoc/>
    protected override void OnUnload()
    {
        _spriteBatch.Dispose();

        base.OnUnload();
    }

    /// <inheritdoc/>
    public override void Draw(SceneDrawContext context)
    {
        if (!IsVisible || Texture == null)
            return;

        _spriteBatch.Begin();

        _spriteBatch.Draw(
            Texture,
            new Vector2(
                _state.Position.X + _state.Offset.X,
                _state.Position.Y + _state.Offset.Y),
            sourceRectangle: TextureRegion != null ?
                new Rectangle(
                    TextureRegion.SourceBounds.X,
                    TextureRegion.SourceBounds.Y,
                    TextureRegion.SourceBounds.Width,
                    TextureRegion.SourceBounds.Height) :
                null,
            color: DiffuseColor * Opacity,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: Vector2.One,
            effects: SpriteEffects.None,
            layerDepth: Layer);

        _spriteBatch.End();
    }
}