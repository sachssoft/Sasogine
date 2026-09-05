using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Defines a backend for triangulating polygon contours.
    /// </summary>
    public interface IPolygonTriangulator
    {
        /// <summary>
        /// Triangulates the specified polygon contours according to the
        /// provided triangulation options.
        /// </summary>
        /// <param name="contours">
        /// The polygon contours to triangulate.
        /// </param>
        /// <param name="options">
        /// The options that control the triangulation process.
        /// </param>
        /// <returns>
        /// A <see cref="PolygonTriangulationResult"/> containing the generated
        /// vertices and triangle indices.
        /// </returns>
        PolygonTriangulationResult Triangulate(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonTriangulationOptions options);
    }
}