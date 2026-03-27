using JetBrains.Annotations;

namespace Box2D;

/// <summary>
/// Base class for unsafe debug draw implementations - i.e. those that use pointers.
/// </summary>
[PublicAPI]
public abstract class DebugDrawUnsafeBase : DebugDraw
{
    private bool initialized;

    /// <summary>
    /// Constructor for DebugDrawUnsafeBase.
    /// </summary>
    protected DebugDrawUnsafeBase()
    { 
        DrawBodyNames = true;
        DrawBounds = true;
        DrawShapes = true;
        DrawJoints = true;
        DrawContactFeatures = true;
        DrawContactImpulses = true;
        DrawContactNormals = true;
        DrawContacts = true;
        DrawGraphColors = true;
        DrawFrictionImpulses = true;
        DrawIslands = true;
        DrawJointExtras = true;
        DrawMass = true;
    }

    internal override unsafe ref DebugDrawInternal Internal
    {
        get
        {
            if (!initialized)
            {
                initialized = true;

                @internal.DrawPolygon = DrawPolygon;
                @internal.DrawSolidPolygon = DrawSolidPolygon;

                @internal.DrawCircle = DrawCircle;
                @internal.DrawSolidCircle = DrawSolidCircle;
                @internal.DrawSolidCapsule = DrawSolidCapsule;
                @internal.DrawSegment = DrawSegment;
                @internal.DrawTransform = DrawTransform;
                @internal.DrawPoint = DrawPoint;
                @internal.DrawString = DrawString;

            }
            return ref @internal;
        }
    }

    /// <summary>
    /// Callback function to draw a closed polygon provided in CCW order.
    /// </summary>
    protected abstract unsafe void DrawPolygon(Vec2* vertices, int vertexCount, HexColor color, nint context);
    /// <summary>
    /// Callback function to draw a solid closed polygon provided in CCW order.
    /// </summary>
    protected abstract unsafe void DrawSolidPolygon(Transform transform, Vec2* vertices, int vertexCount, float radius, HexColor color, nint context);
    /// <summary>
    /// Callback function to draw a circle.
    /// </summary>
    protected abstract void DrawCircle(Vec2 center, float radius, HexColor color, nint context);
    /// <summary>
    /// Callback function to draw a solid circle.
    /// </summary>
    protected abstract void DrawSolidCircle(Transform transform, float radius, HexColor color, nint context);
    /// <summary>
    /// Callback function to draw a solid capsule.
    /// </summary>
    protected abstract void DrawSolidCapsule(Vec2 center1, Vec2 center2, float radius, HexColor color, nint context);
    /// <summary>
    /// Callback function to draw a line segment.
    /// </summary>
    protected abstract void DrawSegment(Vec2 point1, Vec2 point2, HexColor color, nint context);
    /// <summary>
    /// Callback function to draw a transform. Choose your own length scale.
    /// </summary>
    protected abstract void DrawTransform(Transform transform, nint context);
    /// <summary>
    /// Callback function to draw a point.
    /// </summary>
    protected abstract void DrawPoint(Vec2 ppsition, float size, HexColor color, nint context);
    /// <summary>
    /// Callback function to draw a string in world space
    /// </summary>
    protected abstract void DrawString(Vec2 position, nint text, HexColor color, nint context);
}