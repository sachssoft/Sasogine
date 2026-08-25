namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Defines a callback for objects that need to handle
    /// application shutdown events.
    /// </summary>
    public interface IApplicationExitAware
    {
        /// <summary>
        /// Called when the application is exiting.
        /// Allows implementing objects to release resources
        /// or perform cleanup operations.
        /// </summary>
        void OnApplicationExited();
    }
}