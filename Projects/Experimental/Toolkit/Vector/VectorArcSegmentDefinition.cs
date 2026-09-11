namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines the configurable state of an elliptical arc segment.
    /// </summary>
    public sealed class VectorArcSegmentDefinition : VectorFixedSegmentDefinition
    {
        public float RadiusX { get; set; }
        public float RadiusY { get; set; }
        public float Rotation { get; set; }
        public bool LargeArc { get; set; }
        public bool Sweep { get; set; } = true;
    }
}
