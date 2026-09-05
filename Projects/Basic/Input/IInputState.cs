using System;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Defines an input state that provides button state information.
    /// </summary>
    /// <typeparam name="TButton">
    /// The enum type used to identify buttons.
    /// </typeparam>
    public interface IInputState<TButton>
        where TButton : unmanaged, Enum
    {
        /// <summary>
        /// Determines whether the specified button is currently pressed.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>
        /// <see langword="true"/> if the button is pressed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        bool IsButtonDown(TButton button);

        /// <summary>
        /// Determines whether the specified button is currently released.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>
        /// <see langword="true"/> if the button is released; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        bool IsButtonUp(TButton button);
    }
}