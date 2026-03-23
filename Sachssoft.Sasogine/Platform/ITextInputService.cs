using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Platform
{
    /// <summary>
    /// Platform service for prompting the user with a text input dialog.
    /// 
    /// This is mainly relevant for mobile platforms (soft keyboard) or
    /// situations where you want a native input dialog.
    /// On desktop platforms, this can use a standard MessageBox/InputBox or custom UI.
    /// 
    /// Example usage:
    /// <code>
    /// // Ask the user for their name
    /// string? name = await textInputService.PromptAsync(
    ///     "Please enter your name:",
    ///     "Player Name",
    ///     "Guest"
    /// );
    /// 
    /// if (!string.IsNullOrEmpty(name))
    /// {
    ///     messageService.Show($"Hello, {name}!");
    /// }
    /// </code>
    /// </summary>
    public interface ITextInputService
    {
        /// <summary>
        /// Opens a platform-native text input dialog and returns the user input.
        /// Returns null if the user cancels the dialog.
        /// 
        /// Example:
        /// <code>
        /// string? result = await textInputService.PromptAsync("Enter secret code:", "Code", "");
        /// </code>
        /// </summary>
        /// <param name="message">Message or prompt to show to the user.</param>
        /// <param name="title">Optional title of the dialog.</param>
        /// <param name="defaultText">Optional default text in the input field.</param>
        /// <returns>User-entered text, or null if canceled.</returns>
        Task<string?> PromptAsync(string message, string title = "", string defaultText = "");
    }
}