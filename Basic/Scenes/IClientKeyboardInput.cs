using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Defines callbacks for receiving keyboard input events from the client.
    /// Provides support for key state changes and text input handling.
    /// </summary>
    public interface IClientKeyboardInput
    {
        /// <summary>
        /// Gets a value indicating whether text input was received during the current input cycle.
        /// </summary>
        bool HasTextInput { get; }

        /// <summary>
        /// Gets the character received during the last text input event.
        /// </summary>
        char TextInputCharacter { get; }


        /// <summary>
        /// Called when a key is pressed.
        /// Connected to <see cref="GameWindow.KeyDown"/>.
        /// </summary>
        /// <param name="key">
        /// The key that was pressed.
        /// </param>
        void OnKeyDown(Keys key);


        /// <summary>
        /// Called when a key is released.
        /// Connected to <see cref="GameWindow.KeyUp"/>.
        /// </summary>
        /// <param name="key">
        /// The key that was released.
        /// </param>
        void OnKeyUp(Keys key);


        /// <summary>
        /// Called when text input occurs.
        /// Handles character input independently from physical keys.
        /// Connected to <see cref="GameWindow.TextInput"/>.
        /// </summary>
        /// <param name="character">
        /// The character entered by the user.
        /// </param>
        void OnTextInput(char character);
    }
}