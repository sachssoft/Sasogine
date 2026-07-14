using JetBrains.Annotations;

namespace Box2D;

/// <summary>
/// Joint type enumeration
/// </summary>
[PublicAPI]
public enum JointType
{
    /// <summary>
    /// Distance joint
    /// </summary>
    Distance,
    /// <summary>
    /// Filter joint
    /// </summary>
    Filter,
    /// <summary>
    /// Motor joint
    /// </summary>
    Motor,
    /// <summary>
    /// Mouse joint
    /// </summary>
    Mouse,
    /// <summary>
    /// Prismatic joint
    /// </summary>
    Prismatic,
    /// <summary>
    /// Revolute joint
    /// </summary>
    Revolute,
    /// <summary>
    /// Weld joint
    /// </summary>
    Weld,
    /// <summary>
    /// Wheel joint
    /// </summary>
    Wheel,
}