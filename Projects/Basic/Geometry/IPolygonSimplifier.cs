using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Defines a backend for simplifying polygon contours.
    /// </summary>
    public interface IPolygonSimplifier
    {
        /// <summary>
        /// Simplifies the specified polygon contours according to the
        /// provided simplification options.
        /// </summary>
        /// <param name="contours">
        /// The polygon contours to simplify.
        /// </param>
        /// <param name="options">
        /// The options that control the simplification process.
        /// </param>
        /// <returns>
        /// The simplified polygon contours.
        /// </returns>
        IReadOnlyList<IReadOnlyList<Vector2>> Simplify(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonSimplificationOptions options);
    }
}