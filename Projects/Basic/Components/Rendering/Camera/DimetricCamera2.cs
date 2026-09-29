using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics.Cameras;
using System;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Provides a dimetric two-dimensional camera.
/// </summary>
/// <remarks>
/// The camera uses a fixed 45-degree azimuth so that the projected X and Y
/// axes have equal foreshortening, while the Z axis may use a different
/// foreshortening determined by the elevation.
/// </remarks>
public sealed class DimetricCamera2 : AxonometricCamera2
{
    private const float ElevationEpsilon = 0.0001f;

    private float _elevation = MathHelper.ToRadians(30f);

    /// <summary>
    /// Initializes a new dimetric camera.
    /// </summary>
    public DimetricCamera2()
    {
    }

    /// <summary>
    /// Gets or sets the viewing elevation above the XY plane in radians.
    /// </summary>
    public float Elevation
    {
        get => _elevation;
        set
        {
            if (!float.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value));

            var elevation = MathHelper.Clamp(
                value,
                ElevationEpsilon,
                MathHelper.PiOver2 - ElevationEpsilon);

            if (_elevation == elevation)
                return;

            _elevation = elevation;
            UpdateMatrices();
        }
    }

    /// <inheritdoc />
    protected override float ProjectionAzimuth => MathHelper.PiOver4;

    /// <inheritdoc />
    protected override float ProjectionElevation => Elevation;

    /// <summary>
    /// Creates a copy of the camera including its current state.
    /// </summary>
    /// <returns>
    /// A new dimetric camera containing the same configuration and state.
    /// </returns>
    public override ICamera Clone()
    {
        var camera = (DimetricCamera2)base.Clone();

        camera.Elevation = Elevation;

        return camera;
    }

    /// <summary>
    /// Creates the camera instance used when cloning this camera.
    /// </summary>
    /// <returns>
    /// A new dimetric camera instance.
    /// </returns>
    protected override Camera2 CreateClone()
    {
        return new DimetricCamera2();
    }
}