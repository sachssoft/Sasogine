using Sachssoft.Sasogine.Input;

namespace Sachssoft.Sasogine.Services.Platform
{
    /// <summary>
    /// Provides platform-specific information and handling for keyboard modifier keys.
    /// </summary>
    /// <remarks>
    /// This service abstracts differences between platforms, such as
    /// Ctrl on Windows/Linux and Command on macOS, and provides
    /// modifier names and shortcut display formatting.
    /// </remarks>
    public interface IPlatformKeyModifiers
    {
        /// <summary>
        /// Gets the platform-specific display name of a modifier key.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the modifier.
        /// </param>
        /// <returns>
        /// The display string of the modifier, for example "Ctrl", "Cmd", "Alt" or "Shift".
        /// </returns>
        string GetModifierString(int index);

        /// <summary>
        /// Gets the number of supported keyboard modifiers.
        /// </summary>
        int ModifierCount { get; }

        /// <summary>
        /// Determines whether the specified modifier key is currently pressed.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the modifier.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the modifier is pressed; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsModifierPressed(int index);

        /// <summary>
        /// Converts a shortcut into a platform-specific display string.
        /// </summary>
        /// <param name="shortcut">
        /// The shortcut to convert.
        /// </param>
        /// <returns>
        /// A formatted shortcut string using the current platform conventions.
        /// </returns>
        string ToString(Shortcut shortcut);
    }
}