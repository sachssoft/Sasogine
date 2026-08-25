namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Defines callbacks for objects that need to respond to
    /// client activation and deactivation events.
    /// </summary>
    public interface IClientActivator
    {
        /// <summary>
        /// Gets a value indicating whether the client has been activated.
        /// </summary>
        bool WasActivated { get; }


        /// <summary>
        /// Gets a value indicating whether the client has been deactivated.
        /// </summary>
        bool WasDeactivated { get; }


        /// <summary>
        /// Called when the client becomes active.
        /// Allows implementing objects to initialize or resume client-related state.
        /// </summary>
        void OnClientActivate();


        /// <summary>
        /// Called when the client becomes inactive.
        /// Allows implementing objects to pause or release client-related state.
        /// </summary>
        void OnClientDeactivate();
    }
}