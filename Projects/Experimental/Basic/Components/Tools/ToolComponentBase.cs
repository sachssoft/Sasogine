using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Scenes;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    /// <summary>
    /// Provides a base component for managing interactive tools.
    /// </summary>
    public abstract class ToolComponentBase :
        ComponentBase,
        IResourceComponent,
        IDrawableComponent
    {
        private readonly List<ToolBase> _tools = new();

        /// <summary>
        /// Gets a value indicating whether the component resources are loaded.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets the collection of tools managed by this component.
        /// </summary>
        protected IList<ToolBase> Tools => _tools;

        /// <summary>
        /// Loads the resources used by this component and its tools.
        /// </summary>
        public void Load()
        {
            if (IsLoaded)
                return;

            OnLoad();

            for (int i = 0; i < _tools.Count; i++)
            {
                _tools[i].Load();
            }

            IsLoaded = true;
        }

        /// <summary>
        /// Unloads the resources used by this component and its tools.
        /// </summary>
        public void Unload()
        {
            if (!IsLoaded)
                return;

            for (int i = _tools.Count - 1; i >= 0; i--)
            {
                _tools[i].Unload();
            }

            OnUnload();

            IsLoaded = false;
        }

        /// <summary>
        /// Called when the component resources are being loaded.
        /// </summary>
        protected virtual void OnLoad()
        {
        }

        /// <summary>
        /// Called when the component resources are being unloaded.
        /// </summary>
        protected virtual void OnUnload()
        {
        }

        /// <summary>
        /// Updates the component services and all enabled tools using the
        /// specified scene update context.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        public override void Update(SceneUpdateContext context)
        {
            base.Update(context);

            ToolCursorContext cursorContext =
                CreateCursorContext(context);

            var interactions = new ToolInteractions();

            for (int i = 0; i < _tools.Count; i++)
            {
                ToolBase tool = _tools[i];

                if (!tool.IsEnabled)
                    continue;

                interactions.Reset();

                //ApplyInteractions(
                //    tool,
                //    interactions);

                tool.ApplyCursor(cursorContext);
                tool.ApplyInteractions(interactions);
                tool.Update(context);
            }
        }

        /// <summary>
        /// Draws all enabled tools using the specified scene draw context.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene draw operation.
        /// </param>
        public virtual void Draw(SceneDrawContext context)
        {
            for (int i = 0; i < _tools.Count; i++)
            {
                ToolBase tool = _tools[i];

                if (tool.IsEnabled)
                    tool.Draw(context);
            }
        }

        /// <summary>
        /// Creates the cursor context for the current tool update.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        /// <returns>
        /// The cursor context to apply to enabled tools.
        /// </returns>
        protected abstract ToolCursorContext CreateCursorContext(
            SceneUpdateContext context);

        ///// <summary>
        ///// Applies the interaction states for the specified tool.
        ///// </summary>
        ///// <param name="tool">
        ///// The tool for which interaction states are being applied.
        ///// </param>
        ///// <param name="interactions">
        ///// The interaction states to populate.
        ///// </param>
        //protected virtual void ApplyInteractions(
        //    ToolBase tool,
        //    ToolInteractions interactions)
        //{
        //}
    }
}