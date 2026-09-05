using Sachssoft.Sasogine.Components.Tools;
using Sachssoft.Sasogine.Input;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Tools
{
    /// <summary>
    /// Provides the base implementation for interactive tools.
    /// </summary>
    public abstract class ToolBase
    {
        private ToolInteractions? _interactions;

        /// <summary>
        /// Gets or sets a value indicating whether the tool is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets custom data associated with the tool.
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tool participates in input capture.
        /// </summary>
        /// <remarks>
        /// When enabled, the tool receives interactions only when no tool has captured
        /// the input or when this tool is the captured tool.
        /// </remarks>
        public bool UseInputCapture { get; set; } = true;

        /// <summary>
        /// Gets the interaction states used by the tool.
        /// </summary>
        protected internal ToolInteractions Interactions =>
            _interactions ??= CreateInteractions();

        /// <summary>
        /// Loads resources used by the tool.
        /// </summary>
        public virtual void Load()
        {
        }

        /// <summary>
        /// Unloads resources used by the tool.
        /// </summary>
        public virtual void Unload()
        {
        }

        /// <summary>
        /// Updates the tool using the specified scene update context.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        public virtual void Update(SceneUpdateContext context)
        {
        }

        /// <summary>
        /// Draws the tool using the specified scene draw context.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene draw operation.
        /// </param>
        public virtual void Draw(SceneDrawContext context)
        {
        }

        /// <summary>
        /// Creates the interaction states used by the tool.
        /// </summary>
        /// <returns>
        /// The interaction states used by the tool.
        /// </returns>
        protected virtual ToolInteractions CreateInteractions()
        {
            return new ToolInteractions();
        }

        /// <summary>
        /// Applies the current tool context.
        /// </summary>
        /// <param name="context">
        /// Provides the state required for the current tool update.
        /// </param>
        protected internal virtual void ApplyContext(
            ToolContext context)
        {
        }
    }
}