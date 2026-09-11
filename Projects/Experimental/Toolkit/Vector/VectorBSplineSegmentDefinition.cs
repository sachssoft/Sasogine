namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines the configurable state of a B-spline segment.
    /// </summary>
    public sealed class VectorBSplineSegmentDefinition : VectorVariableSegmentDefinition
    {
        public int Degree { get; set; } = 3;
    }
}
