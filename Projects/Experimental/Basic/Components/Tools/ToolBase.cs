using Sachssoft.Sasogine.Components.Tools;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    /// <summary>
    /// Provides the base implementation for interactive tools.
    /// </summary>
    public abstract class ToolBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether the tool is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets custom data associated with the tool.
        /// </summary>
        public object? Tag { get; set; }

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
        /// Applies the current interaction states to the tool.
        /// </summary>
        /// <param name="interactions">
        /// Provides the interaction states to apply.
        /// </param>
        internal protected virtual void ApplyInteractions(
            ToolInteractions interactions)
        {
        }

        /// <summary>
        /// Applies the current cursor state to the tool.
        /// </summary>
        /// <param name="cursorContext">
        /// Provides the cursor state to apply.
        /// </param>
        internal protected virtual void ApplyCursor(
            ToolCursorContext cursorContext)
        {
        }
    }
}