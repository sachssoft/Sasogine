using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Represents a complete 2D transformation for rendering quad-based objects.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="QuadTransform"/> defines how a quad is transformed before
/// being rendered. It contains position, scale, rotation, origin and optional
/// skew information.
/// </para>
///
/// <para>
/// The transformation is applied in local quad space:
/// </para>
///
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="Origin"/> defines the local transformation center.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="Scale"/> changes the size of the quad.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="Skew"/> applies an optional shear deformation.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="Rotation"/> rotates the quad around the origin.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="Position"/> moves the final quad into world space.
/// </description>
/// </item>
/// </list>
///
/// <para>
/// The origin is local to the quad and is independent from tile grid
/// positioning. Tile systems may have their own grid pivot concept.
/// </para>
/// </remarks>
public readonly struct QuadTransform
{
    /// <summary>
    /// Creates an identity transformation.
    /// </summary>
    /// <remarks>
    /// Creates a transformation with:
    /// <list type="bullet">
    /// <item><description>Position = (0,0)</description></item>
    /// <item><description>Scale = (1,1)</description></item>
    /// <item><description>Origin = (0,0)</description></item>
    /// <item><description>Rotation = 0</description></item>
    /// <item><description>Skew = (0,0)</description></item>
    /// </list>
    /// </remarks>
    public QuadTransform()
    {
        Position = Vector2.Zero;
        Scale = Vector2.One;
        Origin = Vector2.Zero;
        Rotation = 0f;
        Skew = Vector2.Zero;
    }


    /// <summary>
    /// Creates a transformation with position and scale.
    /// </summary>
    /// <param name="position">
    /// World position of the quad.
    /// </param>
    /// <param name="scale">
    /// Local scale factor of the quad.
    /// </param>
    public QuadTransform(
        Vector2 position,
        Vector2 scale)
    {
        Position = position;
        Scale = scale;
        Origin = Vector2.Zero;
        Rotation = 0f;
        Skew = Vector2.Zero;
    }


    /// <summary>
    /// Creates a transformation with position, scale and rotation.
    /// </summary>
    /// <param name="position">
    /// World position of the quad.
    /// </param>
    /// <param name="scale">
    /// Local scale factor of the quad.
    /// </param>
    /// <param name="rotation">
    /// Rotation angle in radians.
    /// </param>
    public QuadTransform(
        Vector2 position,
        Vector2 scale,
        float rotation)
    {
        Position = position;
        Scale = scale;
        Origin = Vector2.Zero;
        Rotation = rotation;
        Skew = Vector2.Zero;
    }


    /// <summary>
    /// Creates a complete quad transformation.
    /// </summary>
    /// <param name="position">
    /// World position of the quad.
    /// </param>
    /// <param name="scale">
    /// Local scale factor of the quad.
    /// </param>
    /// <param name="rotation">
    /// Rotation angle in radians.
    /// </param>
    /// <param name="origin">
    /// Local point used as rotation and scaling center.
    /// </param>
    public QuadTransform(
        Vector2 position,
        Vector2 scale,
        float rotation,
        Vector2 origin)
    {
        Position = position;
        Scale = scale;
        Rotation = rotation;
        Origin = origin;
        Skew = Vector2.Zero;
    }


    /// <summary>
    /// Creates a transformation with only a world position.
    /// </summary>
    /// <param name="position">
    /// World position of the quad.
    /// </param>
    public QuadTransform(
        Vector2 position)
    {
        Position = position;
        Scale = Vector2.One;
        Origin = Vector2.Zero;
        Rotation = 0f;
        Skew = Vector2.Zero;
    }


    /// <summary>
    /// Gets an identity transformation.
    /// </summary>
    public static readonly QuadTransform Identity = new();


    /// <summary>
    /// Gets the world position of the quad.
    /// </summary>
    /// <remarks>
    /// Position represents the final location after all local transformations
    /// have been applied.
    /// </remarks>
    public Vector2 Position { get; init; }


    /// <summary>
    /// Gets the local scale of the quad.
    /// </summary>
    /// <remarks>
    /// A value of <see cref="Vector2.One"/> keeps the original size.
    /// Values greater than one enlarge the quad.
    /// Values between zero and one reduce the size.
    /// </remarks>
    public Vector2 Scale { get; init; }


    /// <summary>
    /// Gets the local origin point used for rotation and scaling.
    /// </summary>
    /// <remarks>
    /// The origin is specified in local quad coordinates.
    ///
    /// Examples:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// (0,0) = top-left corner
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// (width / 2, height / 2) = center of the quad
    /// </description>
    /// </item>
    /// </list>
    ///
    /// Unlike a tile grid pivot, this value only affects the local
    /// transformation of the rendered quad.
    /// </remarks>
    public Vector2 Origin { get; init; }


    /// <summary>
    /// Gets the rotation angle of the quad.
    /// </summary>
    /// <remarks>
    /// The value is specified in radians.
    /// Rotation is performed around <see cref="Origin"/>.
    /// </remarks>
    public float Rotation { get; init; }


    /// <summary>
    /// Gets the local skew/shear transformation.
    /// </summary>
    /// <remarks>
    /// Skew applies a horizontal and vertical shear deformation.
    ///
    /// A value of <see cref="Vector2.Zero"/> disables skew processing.
    ///
    /// Skew is mainly intended for special visual effects and is not
    /// commonly required for normal sprite rendering.
    /// </remarks>
    public Vector2 Skew { get; init; }


    /// <summary>
    /// Converts this transformation into a graphics matrix.
    /// </summary>
    /// <remarks>
    /// The matrix is generated in the following order:
    ///
    /// <list type="number">
    /// <item>
    /// <description>
    /// Move the local origin to the coordinate origin.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Apply scale.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Apply skew.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Apply rotation.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Move the quad into world space.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// Default transformation values are skipped where possible to avoid
    /// unnecessary matrix calculations. This is important for batch rendering,
    /// where thousands of quads may be transformed every frame.
    /// </remarks>
    /// <returns>
    /// A matrix representing the complete transformation.
    /// </returns>
    public Matrix ToMatrix()
    {
        // Aufbau der Transformationsmatrix:
        // Origin -> Scale -> Skew -> Rotation -> Position.
        //
        // Standardwerte werden übersprungen, um unnötige Matrix-Berechnungen
        // beim Batch-Rendering vieler Quads zu vermeiden.

        Matrix matrix = Matrix.Identity;


        if (Origin != Vector2.Zero)
        {
            matrix *= Matrix.CreateTranslation(
                -Origin.X,
                -Origin.Y,
                0f);
        }


        if (Scale != Vector2.One)
        {
            matrix *= Matrix.CreateScale(
                Scale.X,
                Scale.Y,
                1f);
        }


        if (Skew != Vector2.Zero)
        {
            matrix *= CreateSkew(
                Skew.X,
                Skew.Y);
        }


        if (Rotation != 0f)
        {
            matrix *= Matrix.CreateRotationZ(
                Rotation);
        }


        if (Position != Vector2.Zero)
        {
            matrix *= Matrix.CreateTranslation(
                Position.X,
                Position.Y,
                0f);
        }


        return matrix;
    }


    /// <summary>
    /// Creates a 2D skew/shear matrix.
    /// </summary>
    /// <param name="skewX">
    /// Horizontal shear amount.
    /// </param>
    /// <param name="skewY">
    /// Vertical shear amount.
    /// </param>
    /// <returns>
    /// A matrix containing the shear transformation.
    /// </returns>
    private static Matrix CreateSkew(
        float skewX,
        float skewY)
    {
        return new Matrix(
            1f, skewY, 0f, 0f,
            skewX, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f);
    }
}