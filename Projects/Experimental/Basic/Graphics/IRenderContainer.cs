using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Graphics
{
    // Für UI empfohlen z.b. Widget mit IRenderContainer,
    // um die Position des Cursors in der UI zu bestimmen.

    /// <summary>
    /// Defines a container that hosts a render surface and provides information
    /// required to map screen coordinates to the local render area.
    /// </summary>
    public interface IRenderContainer
    {
        /// <summary>
        /// Gets the bounds of the container in screen coordinates.
        /// </summary>
        PixelBounds2 Bounds { get; }

        /// <summary>
        /// Gets the size of the render surface in pixels.
        /// </summary>
        PixelSize2 RenderSize { get; }

        /// <summary>
        /// Converts a global screen position to a local position within the render container.
        /// </summary>
        /// <param name="screenGlobalPosition">
        /// The global position on the screen.
        /// </param>
        /// <returns>
        /// The position relative to the top-left corner of the render container.
        /// </returns>
        Point GetLocalPosition(Point screenGlobalPosition);
    }
}