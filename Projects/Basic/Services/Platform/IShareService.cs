namespace Sachssoft.Sasogine.Services.Platform
{
    /// <summary>
    /// Platform service to share text or files using the device's native sharing functionality.
    /// 
    /// Typically used for:
    /// - Sharing a score or screenshot on social media
    /// - Sending a file via email, messenger, or cloud apps
    /// 
    /// Desktop platforms may implement this with default mail clients or clipboard copy.
    /// Mobile platforms (Android/iOS) use the OS share sheet / share intent.
    /// 
    /// Example usage:
    /// <code>
    /// // Share a text message
    /// shareService.ShareText("Check out my score in MinerMania!", "Share Score");
    /// 
    /// // Share a file, e.g., a screenshot
    /// shareService.ShareFile("/storage/emulated/0/Minermania/screenshots/level1.png", "Share Screenshot");
    /// </code>
    /// </summary>
    public interface IShareService
    {
        /// <summary>
        /// Shares plain text via the native OS sharing functionality.
        /// </summary>
        /// <param name="text">The text to share.</param>
        /// <param name="title">Optional title for the sharing dialog.</param>
        void ShareText(string text, string title = "");

        /// <summary>
        /// Shares a file via the native OS sharing functionality.
        /// </summary>
        /// <param name="filePath">The full path of the file to share.</param>
        /// <param name="title">Optional title for the sharing dialog.</param>
        void ShareFile(string filePath, string title = "");
    }
}