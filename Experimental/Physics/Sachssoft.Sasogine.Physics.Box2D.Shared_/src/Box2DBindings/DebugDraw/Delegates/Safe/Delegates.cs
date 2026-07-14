using System;

namespace Box2D.Delegates.Safe;

/// <summary>
/// Draw a circle.
/// </summary>
/// <param name="center">The circle center</param>
/// <param name="radius">The radius</param>
/// <param name="color">The color</param>
public delegate void DrawCircleDelegateSafe(Vec2 center, float radius, HexColor color);

/// <summary>
/// Draw a point.
/// </summary>
/// <param name="p">The point</param>
/// <param name="size">The size</param>
/// <param name="color">The color</param>
public delegate void DrawPointDelegateSafe(Vec2 p, float size, HexColor color);

/// <summary>
/// Draw a closed polygon provided in CCW order.
/// </summary>
/// <param name="vertices">The vertices</param>
/// <param name="color">The color</param>
public delegate void DrawPolygonDelegateSafe(ReadOnlySpan<Vec2> vertices, HexColor color);

/// <summary>
/// Draw a line segment.
/// </summary>
/// <param name="p1">The first point</param>
/// <param name="p2">The second point</param>
/// <param name="color">The color</param>
public delegate void DrawSegmentDelegateSafe(Vec2 p1, Vec2 p2, HexColor color);

/// <summary>
/// Draw a solid capsule.
/// </summary>
/// <param name="p1">The first point</param>
/// <param name="p2">The second point</param>
/// <param name="radius">The radius</param>
/// <param name="color">The color</param>
public delegate void DrawSolidCapsuleDelegateSafe(Vec2 p1, Vec2 p2, float radius, HexColor color);
    
    
/// <summary>
/// Draw a solid circle.
/// </summary>
/// <param name="transform">The transform</param>
/// <param name="radius">The radius</param>
/// <param name="color">The color</param>
public delegate void DrawSolidCircleDelegateSafe(Transform transform, float radius, HexColor color);
    
    
/// <summary>
/// Draw a solid closed polygon provided in CCW order.
/// </summary>
/// <param name="transform">The transform</param>
/// <param name="vertices">The vertices</param>
/// <param name="radius">The radius</param>
/// <param name="color">The color</param>
public delegate void DrawSolidPolygonDelegateSafe(Transform transform, ReadOnlySpan<Vec2> vertices, float radius, HexColor color);
    
/// <summary>
/// Draw a string in world space
/// </summary>
/// <param name="position">The position of the text</param>
/// <param name="text">The string</param>
/// <param name="color">The color</param>
public delegate void DrawStringDelegateSafe(Vec2 position, string? text, HexColor color);
    
    
/// <summary>
/// Draw a transform.
/// </summary>
/// <param name="transform">The transform</param>
/// <remarks>Choose your own length scale</remarks>
public delegate void DrawTransformDelegateSafe(Transform transform);