using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Scenes
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
        /// Changes the active scene after the specified transition completes.
        /// The scene switch takes place on the next scene-manager update frame.
        /// </summary>
        /// <param name="newScene">The scene to activate.</param>
        /// <param name="transition">The transition to update and draw while waiting.</param>
        /// <returns>A task that completes when the scene has been switched.</returns>
        Task ChangeSceneAsync(IScene newScene, ITransition transition);


        /// <summary>
        /// Loads the scene manager and initializes required resources.
        /// </summary>
        void Load();

        /// <summary>Stops the current scene and unloads all active scenes.</summary>
        /// <remarks>Override to release partially loaded scenes and reset manager-specific state.</remarks>
        void Unload()
        {
            if (!IsLoaded)
                return;

            IScene current = CurrentScene;
            IScene[] others = ActiveScenes.Where(scene => !ReferenceEquals(scene, current))
                .Distinct<IScene>(ReferenceEqualityComparer.Instance).ToArray();
            List<Exception>? failures = null;
            Attempt(current.Exit);
            Attempt(current.Unload);
            foreach (IScene scene in others)
                Attempt(scene.Unload);
            if (failures is not null)
                throw new AggregateException("Scene cleanup failed.", failures);

            void Attempt(Action action)
            {
                try { action(); }
                catch (Exception error) { (failures ??= new()).Add(error); }
            }
        }


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
