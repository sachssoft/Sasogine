using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Rendering.Batches;
using System;

namespace Sachssoft.Engine.Graphics.Brushes;

/// <summary>
/// Defines a brush that renders shape geometry using a texture.
/// </summary>
public sealed class TextureBrush : Brush
{
    /// <summary>
    /// Initializes a new texture brush.
    /// </summary>
    /// <param name="texture">The texture used by the brush.</param>
    public TextureBrush(Texture2D texture)
    {
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
    }

    /// <summary>
    /// Gets the texture used by the brush.
    /// </summary>
    public Texture2D Texture { get; }

    /// <summary>
    /// Gets or sets the vertex tint applied to the texture.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the coordinate space used to generate texture coordinates.
    /// </summary>
    public BrushCoordinateSpace CoordinateSpace { get; set; } =
        BrushCoordinateSpace.Normalized;

    /// <summary>
    /// Gets or sets the transformation applied to the generated texture coordinates.
    /// </summary>
    public Matrix TextureTransform { get; set; } = Matrix.Identity;

    /// <summary>
    /// Gets or sets the optional texture region used by the brush.
    /// </summary>
    public PixelBounds2? Region { get; set; }

    /// <summary>
    /// Gets or sets the sampler state used by the texture brush.
    /// </summary>
    public SamplerState SamplerState { get; set; } =
        SamplerState.LinearClamp;

    /// <inheritdoc/>
    protected internal override void Apply(
        ref ShapeVertex vertex,
        in BrushVertexContext context)
    {
        Vector2 textureCoordinate =
            context.GetPosition(CoordinateSpace);

        textureCoordinate = Vector2.Transform(
            textureCoordinate,
            TextureTransform);

        if (Region is PixelBounds2 region)
        {
            Vector2 offset = new(
                region.X / (float)Texture.Width,
                region.Y / (float)Texture.Height);

            Vector2 scale = new(
                region.Width / (float)Texture.Width,
                region.Height / (float)Texture.Height);

            textureCoordinate =
                offset +
                textureCoordinate * scale;
        }

        vertex.Color = Color;
        vertex.TextureCoordinate = textureCoordinate;
    }

    /// <inheritdoc/>
    protected internal override void ApplyDeviceState(
        GraphicsDevice graphicsDevice)
    {
        graphicsDevice.Textures[0] = Texture;
        graphicsDevice.SamplerStates[0] = SamplerState;
    }

    /// <inheritdoc/>
    protected internal override bool CanBatchWith(Brush other) =>
        other is TextureBrush textureBrush &&
        ReferenceEquals(Texture, textureBrush.Texture) &&
        ReferenceEquals(SamplerState, textureBrush.SamplerState);
}