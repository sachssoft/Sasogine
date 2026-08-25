namespace Sachssoft.Sasogine.Geometry
{
    public sealed class PolygonTriangulationOptions
    {
        public PolygonWindingRule WindingRule { get; set; } =
            PolygonWindingRule.EvenOdd;
    }
}
