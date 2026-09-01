using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Experimental.Input;
using Sachssoft.Sasogine.Graphics.Cameras;
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
        /// Updates all enabled tools for each camera associated with the current
        /// scene update.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update and its cameras.
        /// </param>
        public override void Update(SceneUpdateContext context)
        {
            base.Update(context);

            var interactions = new ToolInteractions();

            for (int cameraIndex = 0; cameraIndex < context.Cameras.Length; cameraIndex++)
            {
                ICamera camera = context.Cameras[cameraIndex];

                ICursorState cursorState =
                    GetCursorState(
                        context,
                        camera);

                for (int i = 0; i < _tools.Count; i++)
                {
                    ToolBase tool = _tools[i];

                    if (!tool.IsEnabled)
                        continue;

                    interactions.Reset();

                    tool.ApplyCursor(
                        cursorState,
                        camera);

                    tool.ApplyInteractions(interactions);
                    tool.Update(context);
                }
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
        /// Gets the cursor state used for the current tool update and camera.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        /// <param name="camera">
        /// The camera for which the cursor state is requested.
        /// </param>
        /// <returns>
        /// The cursor state associated with the specified camera.
        /// </returns>
        protected abstract ICursorState GetCursorState(
            SceneUpdateContext context,
            ICamera camera);
    }
}