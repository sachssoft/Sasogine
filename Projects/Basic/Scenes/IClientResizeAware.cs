namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Defines callbacks for receiving client window size and orientation changes.
    /// </summary>
    public interface IClientResizeAware
    {
        /// <summary>
        /// Gets a value indicating whether the client size changed
        /// during the current update cycle.
        /// </summary>
        bool HasClientResize { get; }


        /// <summary>
        /// Called when the client window or display size changes.
        /// </summary>
        void OnClientSizeChanged();


        /// <summary>
        /// Called when the client display orientation changes.
        /// </summary>
        void OnOrientationChanged();
    }
}