using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Provides information about a scene activation event.
    /// Contains references to the scene manager, application,
    /// and currently active scenes.
    /// </summary>
    public class SceneEnterEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneEnterEventArgs"/> class.
        /// </summary>
        /// <param name="manager">
        /// The scene manager responsible for the scene transition.
        /// </param>
        /// <param name="application">
        /// The running game application.
        /// </param>
        /// <param name="activeScenes">
        /// The scenes currently active after the transition.
        /// </param>
        public SceneEnterEventArgs(
            SceneManager manager,
            IGameApplication application,
            IEnumerable<IScene> activeScenes)
        {
            Manager = manager;
            Application = application;
            ActiveScenes = activeScenes;
        }


        /// <summary>
        /// Gets the scene manager that triggered the scene activation.
        /// </summary>
        public SceneManager Manager { get; }


        /// <summary>
        /// Gets the current game application instance.
        /// </summary>
        public IGameApplication Application { get; }


        /// <summary>
        /// Gets the scenes that are currently active.
        /// </summary>
        public IEnumerable<IScene> ActiveScenes { get; }
    }
}