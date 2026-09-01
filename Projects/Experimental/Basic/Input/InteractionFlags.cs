using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Experimental.Input
{
    /// <summary>
    /// Represents the current and previous-frame state of an interaction.
    /// </summary>
    [Flags]
    public enum InteractionFlags
    {
        /// <summary>
        /// No interaction state.
        /// </summary>
        None = 0,

        /// <summary>
        /// The interaction is currently pressed.
        /// </summary>
        IsPressed = 1 << 0,

        /// <summary>
        /// The interaction was pressed during the current frame.
        /// </summary>
        WasJustPressed = 1 << 1,

        /// <summary>
        /// The interaction was released during the current frame.
        /// </summary>
        WasJustReleased = 1 << 2
    }
}
