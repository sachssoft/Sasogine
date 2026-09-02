using Sachssoft.Sasogine.Experimental.Input;

namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    /// <summary>
    /// Provides the interaction states used by <see cref="Camera2NavigationTool"/>.
    /// </summary>
    public sealed class Camera2NavigationToolInteractions : ToolInteractions
    {
        /// <summary>
        /// Gets or sets the interaction state used to zoom the camera in.
        /// </summary>
        public InteractionFlags ZoomIn { get; set; }

        /// <summary>
        /// Gets or sets the interaction state used to zoom the camera out.
        /// </summary>
        public InteractionFlags ZoomOut { get; set; }

        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();

            ZoomIn = InteractionFlags.None;
            ZoomOut = InteractionFlags.None;
        }
    }
}