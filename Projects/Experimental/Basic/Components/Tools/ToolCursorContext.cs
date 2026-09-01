using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    /// <summary>
    /// Provides cursor information for tool operations.
    /// </summary>
    public sealed class ToolCursorContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolCursorContext"/> class.
        /// </summary>
        public ToolCursorContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolCursorContext"/> class
        /// with the specified screen position, world position, movement delta,
        /// and viewport state.
        /// </summary>
        /// <param name="screenPosition">
        /// The current cursor position in screen coordinates.
        /// </param>
        /// <param name="worldPosition">
        /// The current cursor position in world coordinates.
        /// </param>
        /// <param name="delta">
        /// The cursor movement delta since the previous update.
        /// </param>
        /// <param name="isInViewport">
        /// Indicates whether the cursor is inside the active viewport.
        /// </param>
        public ToolCursorContext(
            Vector2 screenPosition,
            Vector2 worldPosition,
            Vector2 delta,
            bool isInViewport)
        {
            ScreenPosition = screenPosition;
            WorldPosition = worldPosition;
            Delta = delta;
            IsInViewport = isInViewport;
        }

        /// <summary>
        /// Gets the current cursor position in screen coordinates.
        /// </summary>
        public Vector2 ScreenPosition { get; init; }

        /// <summary>
        /// Gets the current cursor position in world coordinates.
        /// </summary>
        public Vector2 WorldPosition { get; init; }

        /// <summary>
        /// Gets the cursor movement delta since the previous update.
        /// </summary>
        public Vector2 Delta { get; init; }

        /// <summary>
        /// Gets a value indicating whether the cursor is inside the active viewport.
        /// </summary>
        public bool IsInViewport { get; init; }
    }
}