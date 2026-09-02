using Sachssoft.Sasogine.Experimental.Input;

namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    /// <summary>
    /// Provides the common interaction states used by tools.
    /// </summary>
    public class ToolInteractions
    {
        /// <summary>
        /// Gets or sets the primary action state, such as selecting,
        /// inserting, dragging, or confirming an operation.
        /// </summary>
        public InteractionFlags Action { get; set; }

        /// <summary>
        /// Gets or sets the context action state, such as opening a context menu
        /// or performing a secondary tool-specific operation.
        /// </summary>
        public InteractionFlags Context { get; set; }

        /// <summary>
        /// Gets or sets the modifier interaction state, such as enabling snapping,
        /// constraining movement, or changing the behavior of the primary action.
        /// </summary>
        public InteractionFlags Modifier { get; set; }

        /// <summary>
        /// Gets or sets the cancel action state, such as aborting an insertion,
        /// drag, or other active operation.
        /// </summary>
        public InteractionFlags Cancel { get; set; }

        /// <summary>
        /// Resets all interaction states.
        /// </summary>
        public virtual void Reset()
        {
            Action = InteractionFlags.None;
            Context = InteractionFlags.None;
            Modifier = InteractionFlags.None;
            Cancel = InteractionFlags.None;
        }
    }
}