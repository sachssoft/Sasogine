using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Graphics
{
    // Für UI empfohlen z.B. Widget mit IRenderContainer,
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
        /// Converts a global screen position to a local pixel position
        /// within the render container.
        /// </summary>
        /// <param name="screenGlobalPosition">
        /// The global screen position in pixels.
        /// </param>
        /// <returns>
        /// The pixel position relative to the top-left corner of the render container.
        /// </returns>
        PixelPoint2 GetLocalPosition(PixelPoint2 screenGlobalPosition);
    }
}