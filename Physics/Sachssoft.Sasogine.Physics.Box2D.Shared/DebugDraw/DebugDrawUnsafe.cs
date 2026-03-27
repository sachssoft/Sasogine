using Box2D.Delegates.Unsafe;
using JetBrains.Annotations;

namespace Box2D;

/// <summary>
/// This class holds callbacks you can implement to draw a Box2D world.
/// </summary>
[PublicAPI]
public sealed class DebugDrawUnsafe : DebugDraw
{
    internal override ref DebugDrawInternal Internal => ref @internal;
    
    /// <summary>
    /// Callback function to draw a closed polygon provided in CCW order.
    /// </summary>
    public DrawPolygonDelegate DrawPolygon
    {
        set => @internal.DrawPolygon = value;
    }

    /// <summary>
    /// Callback function to draw a solid closed polygon provided in CCW order.
    /// </summary>
    public DrawSolidPolygonDelegate DrawSolidPolygon
    {
        set => @internal.DrawSolidPolygon = value;
    }

    /// <summary>
    /// Callback function to draw a circle.
    /// </summary>
    public DrawCircleDelegate DrawCircle
    {
        set => @internal.DrawCircle = value;
    }

    /// <summary>
    /// Callback function to draw a solid circle.
    /// </summary>
    public DrawSolidCircleDelegate DrawSolidCircle
    {
        set => @internal.DrawSolidCircle = value;
    }
        
    /// <summary>
    /// Callback function to draw a solid capsule.
    /// </summary>
    public DrawSolidCapsuleDelegate DrawSolidCapsule
    {
        set => @internal.DrawSolidCapsule = value;
    }

    /// <summary>
    /// Callback function to draw a line segment.
    /// </summary>
    public DrawSegmentDelegate DrawSegment
    {
        set => @internal.DrawSegment = value;
    }

    /// <summary>
    /// Callback function to draw a transform. Choose your own length scale.
    /// </summary>
    public DrawTransformDelegate DrawTransform
    {
        set => @internal.DrawTransform = value;
    }

    /// <summary>
    /// Callback function to draw a point.
    /// </summary>
    public DrawPointDelegate DrawPoint
    {
        set => @internal.DrawPoint = value;
    }

    /// <summary>
    /// Callback function to draw a string in world space
    /// </summary>
    public DrawStringDelegate DrawString
    {
        set => @internal.DrawString = value;
    }
}