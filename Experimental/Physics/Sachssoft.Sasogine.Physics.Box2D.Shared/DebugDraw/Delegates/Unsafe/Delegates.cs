namespace Box2D.Delegates.Unsafe;

/// <summary>
/// Draw a circle.
/// </summary>
/// <param name="center">The circle center</param>
/// <param name="radius">The radius</param>
/// <param name="color">The color</param>
/// <param name="context">The context</param>
public delegate void DrawCircleDelegate(Vec2 center, float radius, HexColor color, nint context);
    
/// <summary>
/// Draw a point.
/// </summary>
/// <param name="p">The point</param>
/// <param name="size">The size</param>
/// <param name="color">The color</param>
/// <param name="context">The context</param>
public delegate void DrawPointDelegate(Vec2 p, float size, HexColor color, nint context);

/// <summary>
/// Draw a closed polygon provided in CCW order.
/// </summary>
/// <param name="vertices">The vertices</param>
/// <param name="vertexCount">The vertex count</param>
/// <param name="color">The color</param>
/// <param name="context">The context</param>
public unsafe delegate void DrawPolygonDelegate(Vec2* vertices, int vertexCount, HexColor color, nint context);

/// <summary>
/// Draw a line segment.
/// </summary>
/// <param name="p1">The first point</param>
/// <param name="p2">The second point</param>
/// <param name="color">The color</param>
/// <param name="context">The context</param>
public delegate void DrawSegmentDelegate(Vec2 p1, Vec2 p2, HexColor color, nint context);

/// <summary>
/// Draw a solid capsule.
/// </summary>
/// <param name="p1">The first point</param>
/// <param name="p2">The second point</param>
/// <param name="radius">The radius</param>
/// <param name="color">The color</param>
/// <param name="context">The context</param>
public delegate void DrawSolidCapsuleDelegate(Vec2 p1, Vec2 p2, float radius, HexColor color, nint context);

/// <summary>
/// Draw a solid circle.
/// </summary>
/// <param name="transform">The transform</param>
/// <param name="radius">The radius</param>
/// <param name="color">The color</param>
/// <param name="context">The context</param>
public delegate void DrawSolidCircleDelegate(Transform transform, float radius, HexColor color, nint context);

/// <summary>
/// Draw a solid closed polygon provided in CCW order.
/// </summary>
/// <param name="transform">The transform</param>
/// <param name="vertices">The vertices</param>
/// <param name="vertexCount">The vertex count</param>
/// <param name="radius">The radius</param>
/// <param name="color">The color</param>
/// <param name="context">The context</param>
public unsafe delegate void DrawSolidPolygonDelegate(Transform transform, Vec2* vertices, int vertexCount, float radius, HexColor color, nint context);
 
/// <summary>
/// Draw a string in world space
/// </summary>
/// <param name="p">The point</param>
/// <param name="s">The string</param>
/// <param name="color">The color</param>
/// <param name="context">The context</param>
public delegate void DrawStringDelegate(Vec2 p, nint s, HexColor color, nint context);

/// <summary>
/// Draw a transform.
/// </summary>
/// <param name="transform">The transform</param>
/// <param name="context">The context</param>
/// <remarks>Choose your own length scale</remarks>
public delegate void DrawTransformDelegate(Transform transform, nint context);