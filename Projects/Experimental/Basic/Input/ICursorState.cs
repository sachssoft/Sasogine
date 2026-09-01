using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Cameras;

namespace Sachssoft.Sasogine.Experimental.Input
{
    /// <summary>
    /// Provides the current state and coordinate information of a cursor.
    /// </summary>
    public interface ICursorState
    {
        /// <summary>
        /// Gets the cursor position in screen coordinates.
        /// </summary>
        Vector2 ScreenPosition { get; }

        /// <summary>
        /// Gets the cursor position in world coordinates using the specified camera.
        /// </summary>
        /// <param name="camera">
        /// The camera used to transform the screen position into world coordinates.
        /// </param>
        /// <returns>
        /// The cursor position in world coordinates.
        /// </returns>
        Vector2 GetWorldPosition(ICamera camera);

        /// <summary>
        /// Gets the cursor movement since the previous update in screen coordinates.
        /// </summary>
        Vector2 ScreenDelta { get; }

        /// <summary>
        /// Gets a value indicating whether the cursor is inside the active viewport.
        /// </summary>
        bool IsInViewport { get; }
    }
}