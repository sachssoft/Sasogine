namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Defines runtime settings used for executing a scene.
    /// </summary>
    public interface ISceneRuntimeSettings
    {
        /// <summary>
        /// Gets the runtime mode in which the scene is executed.
        /// </summary>
        RuntimeMode RuntimeMode { get; }

        /// <summary>
        /// Gets the optional runtime features enabled for the scene.
        /// </summary>
        RuntimeOptions RuntimeOptions { get; }
    }
}
