using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Input;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools
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
        /// Gets the tool that currently has exclusive input capture.
        /// </summary>
        protected ToolBase? CapturedTool { get; private set; }

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

            ICursorState cursorState =
                GetCursorState(context);

            for (int cameraIndex = 0; cameraIndex < context.Cameras.Length; cameraIndex++)
            {
                ICamera camera = context.Cameras[cameraIndex];

                for (int i = 0; i < _tools.Count; i++)
                {
                    var tool = _tools[i];

                    if (!tool.IsEnabled)
                        continue;

                    var interactions = tool.Interactions;

                    interactions.Reset();

                    if (!tool.UseInputCapture ||
                        CapturedTool == null ||
                        CapturedTool == tool)
                    {
                        ApplyInteractions(
                            tool,
                            interactions);
                    }

                    var toolContext = new ToolContext(
                        cursorState,
                        camera,
                        interactions);

                    tool.ApplyContext(toolContext);
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
        /// Captures input for the specified tool.
        /// </summary>
        /// <param name="tool">
        /// The tool that should receive exclusive input.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tool"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tool"/> is not managed by this component.
        /// </exception>
        protected void CaptureTool(ToolBase tool)
        {
            ArgumentNullException.ThrowIfNull(tool);

            if (!_tools.Contains(tool))
            {
                throw new ArgumentException(
                    "The tool must be managed by this component.",
                    nameof(tool));
            }

            CapturedTool = tool;
        }

        /// <summary>
        /// Applies the current interaction states to the specified tool.
        /// </summary>
        /// <param name="tool">
        /// The tool that receives the interaction states.
        /// </param>
        /// <param name="interactions">
        /// The interaction states to populate for the tool.
        /// </param>
        protected virtual void ApplyInteractions(
            ToolBase tool,
            ToolInteractions interactions)
        {
        }

        /// <summary>
        /// Gets the cursor state used for the current tool update.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        /// <returns>
        /// The cursor state to apply to enabled tools.
        /// </returns>
        protected abstract ICursorState GetCursorState(
            SceneUpdateContext context);
    }
}