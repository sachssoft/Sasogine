namespace Sachssoft.Sasogine.Services.Platform
{
    /// <summary>
    /// Platform service for picking files or folders using native OS dialogs.
    /// 
    /// Mobile platforms (Android/iOS) use Document Picker / Storage Access Framework.
    /// Desktop platforms use OpenFileDialog / FolderBrowserDialog / SaveFileDialog.
    /// Core code only needs to work with FilePickerResult.
    /// </summary>
    public interface IFilePickerService
    {
        /// <summary>
        /// Opens a file selection dialog and returns the selected file(s).
        /// </summary>
        /// <param name="context">Context describing filters, title, initial folder, etc.</param>
        /// <returns>A <see cref="FilePickerResult"/> with the selected file path(s).</returns>
        FilePickerResult OpenFile(FilePickerContext context);

        /// <summary>
        /// Opens a folder selection dialog and returns the selected folder.
        /// </summary>
        /// <param name="context">Context describing title, initial folder, etc.</param>
        /// <returns>A <see cref="FilePickerResult"/> with the selected folder path.</returns>
        FilePickerResult OpenFolder(FilePickerContext context);

        /// <summary>
        /// Opens a save file dialog and returns the selected file path.
        /// </summary>
        /// <param name="context">Context describing default file name, filters, title, initial folder, etc.</param>
        /// <returns>A <see cref="FilePickerResult"/> with the file path chosen by the user.</returns>
        FilePickerResult SaveFile(FilePickerContext context);
    }
}
