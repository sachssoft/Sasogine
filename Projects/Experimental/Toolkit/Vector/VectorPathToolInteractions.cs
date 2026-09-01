using Sachssoft.Sasogine.Input;

namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    public sealed class VectorPathToolInteractions
    {
        /// <summary>
        /// Gets or sets the interaction used for selection, draw and insert.
        /// </summary>
        public InteractionFlags Action { get; set; } = InteractionFlags.None;

        /// <summary>
        /// Gets or sets the interaction used for context actions.
        /// </summary>
        public InteractionFlags Context { get; set; } = InteractionFlags.None;

        /// <summary>
        /// Gets or sets the interaction used to modify the current operation.
        /// </summary>
        public InteractionFlags Modify { get; set; } = InteractionFlags.None;

        /// <summary>
        /// Gets or sets the interaction used for an alternate operation.
        /// </summary>
        public InteractionFlags Alternate { get; set; } = InteractionFlags.None;

        /// <summary>
        /// Gets or sets the interaction used to cancel the current operation.
        /// </summary>
        public InteractionFlags Cancel { get; set; } = InteractionFlags.None;
    }
}