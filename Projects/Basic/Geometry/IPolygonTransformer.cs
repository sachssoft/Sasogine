using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Defines a backend for applying geometric transformations
    /// to polygon contours.
    /// </summary>
    public interface IPolygonTransformer
    {
        /// <summary>
        /// Transforms the specified polygon contours using the provided matrix.
        /// </summary>
        /// <param name="contours">
        /// The polygon contours to transform.
        /// </param>
        /// <param name="transform">
        /// The transformation matrix to apply to each point.
        /// </param>
        /// <returns>
        /// The transformed polygon contours.
        /// </returns>
        IReadOnlyList<IReadOnlyList<Vector2>> Transform(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            Matrix transform);
    }
}