namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines the configurable state of a Catmull-Rom segment.
    /// </summary>
    public sealed class VectorCatmullRomSegmentDefinition : VectorVariableSegmentDefinition
    {
        public bool Closed { get; set; }
    }
}
