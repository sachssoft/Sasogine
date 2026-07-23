using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Provides a transformation matrix representation for rendering operations.
/// </summary>
/// <remarks>
/// Implementations convert their internal transformation data into a
/// <see cref="Matrix"/> used by the graphics pipeline.
/// </remarks>
public interface IMatrixProvider
{
    /// <summary>
    /// Creates the transformation matrix representation.
    /// </summary>
    /// <returns>
    /// A <see cref="Matrix"/> representing this transform.
    /// </returns>
    Matrix ToMatrix();
}