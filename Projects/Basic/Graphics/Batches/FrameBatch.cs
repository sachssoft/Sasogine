using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Graphics.Rendering.Batches;

/// <summary>
/// Renders textured frames from a texture atlas.
/// </summary>
public sealed class FrameBatch : QuadBatchBase
{
    /// <summary>
    /// Creates a new frame batch.
    /// </summary>
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
    /// <param name="sourceBounds">
    /// Pixel bounds inside the texture atlas.
    /// </param>
    /// <param name="color">
    /// Color tint.
    /// </param>
    public void AddFrame(
        Point2 position,
        PixelBounds2 sourceBounds,
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
            sourceBounds,
            color);
    }

    /// <summary>
    /// Adds a transformed frame.
    /// </summary>
    /// <param name="transform">
    /// Transformation applied to the frame.
    /// </param>
    /// <param name="sourceBounds">
    /// Pixel bounds inside the texture atlas.
    /// </param>
    /// <param name="color">
    /// Color tint.
    /// </param>
    public void AddFrame(
        QuadTransform transform,
        PixelBounds2 sourceBounds,
        Color color)
    {
        Matrix matrix = transform.ToMatrix();

        AddQuad(
            matrix,
            sourceBounds,
            color);
    }

    /// <summary>
    /// Creates a centered frame transformation.
    /// </summary>
    /// <param name="position">
    /// World position of the frame center.
    /// </param>
    /// <param name="sourceBounds">
    /// Pixel bounds inside the texture atlas.
    /// </param>
    /// <returns>
    /// Transformation with the origin set to the frame center.
    /// </returns>
    public static QuadTransform CreateCenteredTransform(
        Point2 position,
        PixelBounds2 sourceBounds)
    {
        return new QuadTransform
        {
            Position = position,
            Scale = Vector2.One,
            Rotation = 0f,
            Origin = new Vector2(
                sourceBounds.Width / 2f,
                sourceBounds.Height / 2f)
        };
    }
}