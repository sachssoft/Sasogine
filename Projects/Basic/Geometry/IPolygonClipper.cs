using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Defines a backend for performing clipping operations
    /// on polygon contours.
    /// </summary>
    public interface IPolygonClipper
    {
        /// <summary>
        /// Performs the specified clipping operation between subject
        /// and clipping contours.
        /// </summary>
        /// <param name="subject">
        /// The subject polygon contours to process.
        /// </param>
        /// <param name="clip">
        /// The polygon contours used to clip the subject geometry.
        /// </param>
        /// <param name="operation">
        /// The clipping operation to perform.
        /// </param>
        /// <returns>
        /// The polygon contours produced by the clipping operation.
        /// </returns>
        IReadOnlyList<IReadOnlyList<Vector2>> Clip(
            IReadOnlyList<IReadOnlyList<Vector2>> subject,
            IReadOnlyList<IReadOnlyList<Vector2>> clip,
            PolygonClipOperation operation);
    }
}