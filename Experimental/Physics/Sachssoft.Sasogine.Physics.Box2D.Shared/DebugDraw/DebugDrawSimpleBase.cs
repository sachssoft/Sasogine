using System;
using System.Runtime.InteropServices;

namespace Box2D;

/// <summary>
/// Base class for simple debug draw implementations - i.e. without context.
/// </summary>
public abstract class DebugDrawSimpleBase : DebugDraw
{
    private bool initialized;

    /// <summary>
    /// Constructor for DebugDrawSimpleBase.
    /// </summary>
    protected DebugDrawSimpleBase()
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
                @internal.DrawPolygon = (vertexPtr, vertexCount, color, _) =>
                {
                    var vertices = new ReadOnlySpan<Vec2>(vertexPtr, vertexCount);
                    DrawPolygon(vertices, color);
                };

                @internal.DrawSolidPolygon = (transform, vertexPtr, vertexCount, radius, color, _) =>
                {
                    var vertices = new ReadOnlySpan<Vec2>(vertexPtr, vertexCount);
                    DrawSolidPolygon(transform, vertices, radius, color);
                };

                @internal.DrawCircle = (center, radius, color, _) => DrawCircle(center, radius, color);
                @internal.DrawSolidCircle = (transform, radius, color, _) => DrawSolidCircle(transform, radius, color);
                @internal.DrawSolidCapsule = (center1, center2, radius, color, _) => DrawSolidCapsule(center1, center2, radius, color);
                @internal.DrawSegment = (p1, p2, color, _) => DrawSegment(p1, p2, color);
                @internal.DrawTransform = (transform, _) => DrawTransform(transform);
                @internal.DrawPoint = (p, size, color, _) => DrawPoint(p, size, color);
                @internal.DrawString = (p, s, color, _) =>
                {
                    string? str = Marshal.PtrToStringUTF8(s);
                    DrawString(p, str, color);
                };
            }

            return ref @internal;
        }
    }

    /// <summary>
    /// Callback function to draw a closed polygon provided in CCW order.
    /// </summary>
    protected abstract void DrawPolygon(ReadOnlySpan<Vec2> vertices, HexColor color);

    /// <summary>
    /// Callback function to draw a solid closed polygon provided in CCW order.
    /// </summary>
    protected abstract void DrawSolidPolygon(Transform transform, ReadOnlySpan<Vec2> vertices, float radius, HexColor color);

    /// <summary>
    /// Callback function to draw a circle.
    /// </summary>
    protected abstract void DrawCircle(Vec2 center, float radius, HexColor color);

    /// <summary>
    /// Callback function to draw a solid circle.
    /// </summary>
    protected abstract void DrawSolidCircle(Transform transform, float radius, HexColor color);

    /// <summary>
    /// Callback function to draw a solid capsule.
    /// </summary>
    protected abstract void DrawSolidCapsule(Vec2 p1, Vec2 p2, float radius, HexColor color);

    /// <summary>
    /// Callback function to draw a line segment.
    /// </summary>
    protected abstract void DrawSegment(Vec2 p1, Vec2 p2, HexColor color);

    /// <summary>
    /// Callback function to draw a transform. Choose your own length scale.
    /// </summary>
    protected abstract void DrawTransform(Transform transform);

    /// <summary>
    /// Callback function to draw a point.
    /// </summary>
    protected abstract void DrawPoint(Vec2 p, float size, HexColor color);

    /// <summary>
    /// Callback function to draw a string in world space
    /// </summary>
    protected abstract void DrawString(Vec2 p, string? str, HexColor color);
}