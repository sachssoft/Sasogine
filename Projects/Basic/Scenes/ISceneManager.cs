using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Defines a manager responsible for loading, switching, updating,
    /// and rendering game scenes.
    /// </summary>
    public interface ISceneManager
    {
        /// <summary>
        /// Gets a value indicating whether the scene manager has been loaded.
        /// </summary>
        bool IsLoaded { get; }


        /// <summary>
        /// Gets the currently active scene.
        /// </summary>
        IScene CurrentScene { get; }


        /// <summary>
        /// Gets all currently active scenes.
        /// Supports multiple active scenes such as overlays,
        /// menus, and gameplay layers.
        /// </summary>
        IEnumerable<IScene> ActiveScenes { get; }


        /// <summary>
        /// Changes the active scene.
        /// </summary>
        /// <param name="newScene">
        /// The scene that should become active.
        /// </param>
        void ChangeScene(IScene newScene);


        /// <summary>
        /// Loads the scene manager and initializes required resources.
        /// </summary>
        void Load();


        /// <summary>
        /// Updates all active scenes.
        /// </summary>
        /// <param name="gameTime">
        /// Provides timing information for the current update cycle.
        /// </param>
        void Update(GameTime gameTime);


        /// <summary>
        /// Draws all active scenes.
        /// </summary>
        /// <param name="gameTime">
        /// Provides timing information for the current render cycle.
        /// </param>
        void Draw(GameTime gameTime);
    }
}