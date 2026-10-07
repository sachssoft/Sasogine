using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Rendering;
using Sachssoft.Engine.Graphics.Rendering.Batches;

namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Defines how shape geometry is filled or stroked.
/// </summary>
public abstract class Brush
{
    /// <summary>
    /// Applies brush-specific values to a shape vertex.
    /// </summary>
    /// <param name="vertex">The vertex to modify.</param>
    /// <param name="context">The coordinates associated with the vertex.</param>
    protected internal abstract void Apply(
        ref ShapeVertex vertex,
        in BrushVertexContext context);

    /// <summary>
    /// Configures shader state required by this brush before the shader is applied.
    /// </summary>
    /// <param name="shader">The shader used by the shape batch.</param>
    protected internal virtual void ConfigureShader(IShader shader)
    {
    }

    /// <summary>
    /// Applies graphics-device state required by this brush immediately before drawing.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    protected internal virtual void ApplyDeviceState(GraphicsDevice graphicsDevice)
    {
    }

    /// <summary>
    /// Determines whether this brush can be rendered in the same draw command
    /// as the specified brush.
    /// </summary>
    /// <param name="other">The brush to compare with this brush.</param>
    /// <returns>
    /// <see langword="true"/> when both brushes use compatible render state;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected internal virtual bool CanBatchWith(Brush other) =>
        ReferenceEquals(this, other);
}
