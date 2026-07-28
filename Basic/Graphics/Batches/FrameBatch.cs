using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics.Rendering.Batches;

// FrameBatch ist quasi ein SpriteBatch,
// aber auf Basis der eigenen QuadBatch-Infrastruktur für effizientes Batch-Rendering.

// Anders als TileBatch ist FrameBatch einfacher,
// hat dafür mehr Boiler State durch explizite Transform-Daten.

/// <summary>
/// Renders textured frames from a texture atlas.
/// </summary>
/// <remarks>
/// A frame represents a rectangular region inside a texture.
/// 
/// Unlike tile based batches, this batch does not use a grid coordinate
/// system. Frames are positioned directly in world space.
/// 
/// Transformations are handled by <see cref="QuadTransform"/>.
/// The transform origin defines the local point used for rotation and scaling.
/// </remarks>
public sealed class FrameBatch : QuadBatchBase
{
    /// <summary>
    /// Creates a new frame batch.
    /// </summary>
    /// <param name="graphicsDevice">
    /// Graphics device used for rendering.
    /// </param>
    /// <param name="initialCapacity">
    /// Initial frame capacity.
    /// </param>
    public FrameBatch(
        GraphicsDevice graphicsDevice,
        int initialCapacity = 1024)
        : base(
            graphicsDevice,
            initialCapacity)
    {
    }


    /// <summary>
    /// Adds a frame at a world position.
    /// </summary>
    /// <param name="position">
    /// World position of the frame.
    /// </param>
    /// <param name="sourceRect">
    /// Rectangle inside the texture atlas.
    /// </param>
    /// <param name="color">
    /// Color tint.
    /// </param>
    public void AddFrame(
        Vector2 position,
        Rectangle sourceRect,
        Color color)
    {
        QuadTransform transform =
            new()
            {
                Position = position,
                Scale = Vector2.One,
                Rotation = 0f,
                Origin = Vector2.Zero
            };


        AddFrame(
            transform,
            sourceRect,
            color);
    }


    /// <summary>
    /// Adds a transformed frame.
    /// </summary>
    /// <remarks>
    /// The supplied transform is used directly.
    /// If rotation around the center is required,
    /// set the origin to half of the frame size.
    /// </remarks>
    /// <param name="transform">
    /// Transformation applied to the frame.
    /// </param>
    /// <param name="sourceRect">
    /// Rectangle inside the texture atlas.
    /// </param>
    /// <param name="color">
    /// Color tint.
    /// </param>
    public void AddFrame(
        QuadTransform transform,
        Rectangle sourceRect,
        Color color)
    {
        Matrix matrix =
            transform.ToMatrix();


        AddQuad(
            matrix,
            sourceRect,
            color);
    }


    /// <summary>
    /// Creates a centered frame transformation.
    /// </summary>
    /// <param name="position">
    /// World position of the frame center.
    /// </param>
    /// <param name="sourceRect">
    /// Rectangle inside the texture atlas.
    /// </param>
    /// <returns>
    /// Transformation with the origin set to the frame center.
    /// </returns>
    public static QuadTransform CreateCenteredTransform(
        Vector2 position,
        Rectangle sourceRect)
    {
        return new QuadTransform
        {
            Position = position,
            Scale = Vector2.One,
            Rotation = 0f,
            Origin = new Vector2(
                sourceRect.Width / 2f,
                sourceRect.Height / 2f)
        };
    }
}