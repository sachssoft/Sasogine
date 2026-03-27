global using Vec2 = System.Numerics.Vector2;
global using uint64_t = System.UInt64;
global using static Box2D.Constants;

namespace Box2D;

/// <summary>
/// Constants used by Box2D.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The value of pi, used for various calculations.
    /// </summary>
    public const float PI = 3.14159265358979323846f;
    
    /// <summary>
    /// Default category bits for collision filtering.
    /// </summary>
    public const ulong DEFAULT_CATEGORY_BITS = 0x0001;
    
    /// <summary>
    /// Default mask bits for collision filtering.
    /// </summary>
    public const ulong DEFAULT_MASK_BITS = 0xFFFF;
    
    /// <summary>
    /// Maximum number of polygon vertices.
    /// </summary>
    public const int MAX_POLYGON_VERTICES = 8;
}
