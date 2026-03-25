using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Scenes
{
    public interface IClientKeyboardInput
    {
        /// <summary>
        /// Called when a key is pressed down.
        /// Hooked to GameWindow.KeyDown.
        /// </summary>
        void OnKeyDown(Keys key);

        /// <summary>
        /// Called when a key is released.
        /// Hooked to GameWindow.KeyUp.
        /// </summary>
        void OnKeyUp(Keys key);

        /// <summary>
        /// Called when text input occurs (character input).
        /// Hooked to GameWindow.TextInput.
        /// </summary>
        void OnTextInput(char character);
    }
}
