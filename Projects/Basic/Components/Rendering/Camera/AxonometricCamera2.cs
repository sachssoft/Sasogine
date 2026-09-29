using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using System;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Provides a base implementation for two-dimensional cameras using
/// an axonometric projection.
/// </summary>
/// <remarks>
/// The camera projects the X, Y and Z axes orthographically into the
/// two-dimensional view while retaining the standard position, zoom and
/// rotation behavior provided by <see cref="Camera2"/>.
/// </remarks>
public abstract class AxonometricCamera2 : Camera2
{
    private const float PlaneIntersectionEpsilon = 0.000001f;
    private const float ProjectionDepthRange = 100000f;

    /// <summary>
    /// Gets the horizontal viewing angle around the Z axis in radians.
    /// </summary>
    protected abstract float ProjectionAzimuth { get; }

    /// <summary>
    /// Gets the vertical viewing angle above the XY plane in radians.
    /// </summary>
    protected abstract float ProjectionElevation { get; }

    /// <summary>
    /// Converts a screen-space position into a position on the
    /// two-dimensional world plane at Z = 0.
    /// </summary>
    /// <param name="screenPosition">
    /// The position in screen space.
    /// </param>
    /// <returns>
    /// The corresponding position on the XY world plane.
    /// </returns>
    public override Point2 ToWorld(Point2 screenPosition)
    {
        var near = CurrentViewport.Unproject(
            new Vector3(
                screenPosition.X,
                screenPosition.Y,
                0f),
            Projection,
            View,
            World);

        var far = CurrentViewport.Unproject(
            new Vector3(
                screenPosition.X,
                screenPosition.Y,
                1f),
            Projection,
            View,
            World);

        var direction = far - near;

        if (Math.Abs(direction.Z) < PlaneIntersectionEpsilon)
        {
            return new Point2(
                near.X,
                near.Y);
        }

        float distance = -near.Z / direction.Z;
        var position = near + direction * distance;

        return new Point2(
            position.X,
            position.Y);
    }

    /// <summary>
    /// Recalculates the axonometric camera transformation matrices.
    /// </summary>
    protected override void UpdateMatrices()
    {
        if (CurrentViewport.Width <= 0 ||
            CurrentViewport.Height <= 0)
        {
            return;
        }

        Projection = Matrix.CreateOrthographicOffCenter(
            0f,
            CurrentViewport.Width,
            CurrentViewport.Height,
            0f,
            -ProjectionDepthRange,
            ProjectionDepthRange);

        var center = new Vector2(
            CurrentViewport.Width * 0.5f,
            CurrentViewport.Height * 0.5f);

        var axonometricProjection = CreateAxonometricProjectionMatrix(
            ProjectionAzimuth,
            ProjectionElevation);

        float scale = BaseZoomFactor * Zoom;

        View =
            Matrix.CreateTranslation(
                -Position.X,
                -Position.Y,
                0f)
            *
            axonometricProjection
            *
            Matrix.CreateTranslation(
                center.X,
                center.Y,
                0f)
            *
            Matrix.CreateRotationZ(Rotation)
            *
            Matrix.CreateScale(
                scale,
                scale,
                1f)
            *
            Matrix.CreateTranslation(
                -center.X,
                -center.Y,
                0f);

        World = Matrix.Identity;
    }

    private static Matrix CreateAxonometricProjectionMatrix(
        float azimuth,
        float elevation)
    {
        float sinAzimuth = MathF.Sin(azimuth);
        float cosAzimuth = MathF.Cos(azimuth);

        float sinElevation = MathF.Sin(elevation);
        float cosElevation = MathF.Cos(elevation);

        float xAxisX = sinAzimuth;
        float xAxisY = sinElevation * cosAzimuth;

        float xAxisLength = MathF.Sqrt(
            xAxisX * xAxisX +
            xAxisY * xAxisY);

        float normalization =
            xAxisLength > PlaneIntersectionEpsilon
                ? 1f / xAxisLength
                : 1f;

        var matrix = Matrix.Identity;

        // Projected X axis.
        matrix.M11 = sinAzimuth * normalization;
        matrix.M12 = sinElevation * cosAzimuth * normalization;

        // Projected Y axis.
        matrix.M21 = -cosAzimuth * normalization;
        matrix.M22 = sinElevation * sinAzimuth * normalization;

        // Projected Z axis.
        matrix.M31 = 0f;
        matrix.M32 = -cosElevation * normalization;

        return matrix;
    }
}