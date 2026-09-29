using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics.Cameras;
using System;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Provides a configurable trimetric two-dimensional camera.
/// </summary>
/// <remarks>
/// Both azimuth and elevation can be configured independently, allowing
/// the projected X, Y and Z axes to use different foreshortening ratios.
/// </remarks>
public sealed class TrimetricCamera2 : AxonometricCamera2
{
    private const float ElevationEpsilon = 0.0001f;

    private float _azimuth = MathHelper.ToRadians(35f);
    private float _elevation = MathHelper.ToRadians(25f);

    /// <summary>
    /// Initializes a new trimetric camera.
    /// </summary>
    public TrimetricCamera2()
    {
    }

    /// <summary>
    /// Gets or sets the horizontal viewing angle around the Z axis
    /// in radians.
    /// </summary>
    public float Azimuth
    {
        get => _azimuth;
        set
        {
            if (!float.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value));

            var azimuth = MathHelper.WrapAngle(value);

            if (_azimuth == azimuth)
                return;

            _azimuth = azimuth;
            UpdateMatrices();
        }
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
    protected override float ProjectionAzimuth => Azimuth;

    /// <inheritdoc />
    protected override float ProjectionElevation => Elevation;

    /// <summary>
    /// Creates a copy of the camera including its current state.
    /// </summary>
    /// <returns>
    /// A new trimetric camera containing the same configuration and state.
    /// </returns>
    public override ICamera Clone()
    {
        var camera = (TrimetricCamera2)base.Clone();

        camera.Azimuth = Azimuth;
        camera.Elevation = Elevation;

        return camera;
    }

    /// <summary>
    /// Creates the camera instance used when cloning this camera.
    /// </summary>
    /// <returns>
    /// A new trimetric camera instance.
    /// </returns>
    protected override Camera2 CreateClone()
    {
        return new TrimetricCamera2();
    }
}