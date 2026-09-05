namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Defines options used when triangulating polygon geometry.
    /// </summary>
    public sealed class PolygonTriangulationOptions
    {
        /// <summary>
        /// Gets or sets the winding rule used to determine which regions
        /// of the polygon are considered filled during triangulation.
        /// </summary>
        public PolygonWindingRule WindingRule { get; set; } =
            PolygonWindingRule.EvenOdd;
    }
}