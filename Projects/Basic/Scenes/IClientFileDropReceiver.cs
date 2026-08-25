namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Defines a callback for receiving files dropped onto the client application.
    /// </summary>
    public interface IClientFileDropReceiver
    {
        /// <summary>
        /// Called when one or more files are dropped onto the application.
        /// </summary>
        /// <param name="files">
        /// The file paths of the dropped files.
        /// </param>
        void OnFileDrop(string[] files);
    }
}