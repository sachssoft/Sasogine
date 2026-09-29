using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Provides a true isometric two-dimensional camera.
/// </summary>
/// <remarks>
/// The projection uses equal foreshortening for the X, Y and Z axes.
/// The projected X and Y axes form the characteristic 30-degree
/// isometric ground directions.
/// </remarks>
public sealed class IsometricCamera2 : AxonometricCamera2
{
    private static readonly float IsometricElevation =
        MathF.Atan(1f / MathF.Sqrt(2f));

    /// <summary>
    /// Initializes a new isometric camera.
    /// </summary>
    public IsometricCamera2()
    {
    }

    /// <inheritdoc />
    protected override float ProjectionAzimuth => MathHelper.PiOver4;

    /// <inheritdoc />
    protected override float ProjectionElevation => IsometricElevation;

    /// <summary>
    /// Creates the camera instance used when cloning this camera.
    /// </summary>
    /// <returns>
    /// A new isometric camera instance.
    /// </returns>
    protected override Camera2 CreateClone()
    {
        return new IsometricCamera2();
    }
}