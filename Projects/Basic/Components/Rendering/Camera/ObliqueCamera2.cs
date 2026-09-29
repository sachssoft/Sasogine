using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Provides a two-dimensional camera with support for oblique
/// projection of local Z depth into the XY plane.
/// </summary>
public class ObliqueCamera2 : Camera2
{
    private Vector2 _depthProjection = new(0f, -1f);

    /// <summary>
    /// Gets or sets the projection offset applied for each unit
    /// of local Z depth.
    /// </summary>
    public Vector2 DepthProjection
    {
        get => _depthProjection;
        set
        {
            if (!float.IsFinite(value.X) ||
                !float.IsFinite(value.Y))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Depth projection components must be finite.");
            }

            _depthProjection = value;
        }
    }

    /// <summary>
    /// Creates an entity transformation with oblique depth projection
    /// while keeping depth independent of entity scaling and rotation.
    /// </summary>
    public Matrix CreateDepthTransform(Matrix transform)
    {
        if (!transform.Decompose(
            out Vector3 scale,
            out Quaternion rotation,
            out Vector3 translation))
        {
            throw new InvalidOperationException(
                "The transformation could not be decomposed.");
        }

        return
            Matrix.CreateScale(
                scale.X,
                scale.Y,
                1f)
            *
            Matrix.CreateFromQuaternion(rotation)
            *
            CreateDepthProjectionMatrix()
            *
            Matrix.CreateTranslation(translation);
    }

    /// <summary>
    /// Creates the depth projection matrix.
    /// </summary>
    protected virtual Matrix CreateDepthProjectionMatrix()
    {
        var matrix = Matrix.Identity;

        matrix.M31 = -DepthProjection.X;
        matrix.M32 = -DepthProjection.Y;
        matrix.M33 = 0f;

        return matrix;
    }

    /// <inheritdoc/>
    protected override Camera2 CreateClone()
    {
        return new ObliqueCamera2();
    }
}