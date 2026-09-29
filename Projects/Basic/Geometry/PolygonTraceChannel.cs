namespace Sachssoft.Engine.Geometry;

/// <summary>
/// Defines the texture channel used to create the tracing mask.
/// </summary>
public enum PolygonTraceChannel
{
    /// <summary>Uses the alpha channel.</summary>
    Alpha,

    /// <summary>Uses perceived luminance derived from the RGB channels.</summary>
    Luminance,

    /// <summary>Uses the red channel.</summary>
    Red,

    /// <summary>Uses the green channel.</summary>
    Green,

    /// <summary>Uses the blue channel.</summary>
    Blue
}
