namespace Box2D;

/// <summary>
/// The body simulation type.
/// Each body is one of these three types. The type determines how the body behaves in the simulation.
/// </summary>
public enum BodyType
{
    /// <summary>
    /// zero mass, zero velocity, may be manually moved
    /// </summary>
    Static = 0,

    /// <summary>
    /// zero mass, velocity set by user, moved by solver
    /// </summary>
    Kinematic = 1,

    /// <summary>
    /// positive mass, velocity determined by forces, moved by solver
    /// </summary>
    Dynamic = 2
}