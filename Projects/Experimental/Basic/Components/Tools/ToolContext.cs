using Sachssoft.Sasogine.Experimental.Input;
using Sachssoft.Sasogine.Graphics.Cameras;

namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    /// <summary>
    /// Provides the context required by a tool during an update.
    /// </summary>
    /// <remarks>
    /// The context contains the state associated with the current tool update
    /// and can be extended with additional information without changing the
    /// tool context API.
    /// </remarks>
    public sealed class ToolContext
    {
        /// <summary>
        /// Gets the current cursor state.
        /// </summary>
        public ICursorState CursorState { get; }

        /// <summary>
        /// Gets the camera associated with the current view.
        /// </summary>
        public ICamera Camera { get; }

        /// <summary>
        /// Gets the current interaction states.
        /// </summary>
        public ToolInteractions Interactions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolContext"/> class.
        /// </summary>
        /// <param name="cursorState">
        /// The current cursor state.
        /// </param>
        /// <param name="camera">
        /// The camera associated with the current view.
        /// </param>
        /// <param name="interactions">
        /// The current interaction states.
        /// </param>
        public ToolContext(
            ICursorState cursorState,
            ICamera camera,
            ToolInteractions interactions)
        {
            CursorState = cursorState;
            Camera = camera;
            Interactions = interactions;
        }
    }
}