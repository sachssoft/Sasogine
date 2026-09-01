using Sachssoft.Sasogine.Input;

namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    /// <summary>
    /// Provides common interaction states used by interactive tools.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The interaction roles are independent of a specific input device.
    /// Applications may bind keyboard keys, mouse buttons, gamepad buttons,
    /// or other input sources to the same interaction.
    /// </para>
    /// <para>
    /// The examples given for individual interactions are recommendations only.
    /// Actual bindings are defined by the application and may vary between tools.
    /// </para>
    /// </remarks>
    public sealed class ToolInteractions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolInteractions"/> class.
        /// </summary>
        public ToolInteractions()
        {
        }

        /// <summary>
        /// Gets or sets the primary interaction used to perform the main action
        /// of a tool.
        /// </summary>
        /// <remarks>
        /// Typical bindings include the left mouse button, Enter or Space on a
        /// keyboard, and the south face button on a gamepad, such as A on an
        /// Xbox controller.
        /// </remarks>
        public InteractionFlags Primary { get; set; }

        /// <summary>
        /// Gets or sets the secondary interaction used to perform an alternative
        /// or contextual action.
        /// </summary>
        /// <remarks>
        /// Typical bindings include the right mouse button, Escape or another
        /// application-defined key, and the east face button on a gamepad,
        /// such as B on an Xbox controller.
        /// </remarks>
        public InteractionFlags Secondary { get; set; }

        /// <summary>
        /// Gets or sets the tertiary interaction used to perform an additional
        /// tool action.
        /// </summary>
        /// <remarks>
        /// Typical bindings include a keyboard shortcut, an additional mouse
        /// button, and the west face button on a gamepad, such as X on an
        /// Xbox controller.
        /// </remarks>
        public InteractionFlags Tertiary { get; set; }

        /// <summary>
        /// Gets or sets the quaternary interaction used to perform an additional
        /// tool action.
        /// </summary>
        /// <remarks>
        /// Typical bindings include a keyboard shortcut, an additional mouse
        /// button, and the north face button on a gamepad, such as Y on an
        /// Xbox controller.
        /// </remarks>
        public InteractionFlags Quaternary { get; set; }

        /// <summary>
        /// Gets or sets the interaction corresponding to a left-side modifier
        /// or shoulder action.
        /// </summary>
        /// <remarks>
        /// Typical bindings include Shift or Control on a keyboard and the
        /// left shoulder button on a gamepad.
        /// </remarks>
        public InteractionFlags LeftShoulder { get; set; }

        /// <summary>
        /// Gets or sets the interaction corresponding to a right-side modifier
        /// or shoulder action.
        /// </summary>
        /// <remarks>
        /// Typical bindings include Shift or Control on a keyboard and the
        /// right shoulder button on a gamepad.
        /// </remarks>
        public InteractionFlags RightShoulder { get; set; }

        /// <summary>
        /// Gets or sets the interaction corresponding to the left trigger.
        /// </summary>
        /// <remarks>
        /// Typical bindings include a keyboard modifier, a mouse button, or the
        /// left trigger on a gamepad.
        /// This interaction represents only the digital interaction state and
        /// does not provide an analog trigger value.
        /// </remarks>
        public InteractionFlags LeftTrigger { get; set; }

        /// <summary>
        /// Gets or sets the interaction corresponding to the right trigger.
        /// </summary>
        /// <remarks>
        /// Typical bindings include a keyboard modifier, a mouse button, or the
        /// right trigger on a gamepad.
        /// This interaction represents only the digital interaction state and
        /// does not provide an analog trigger value.
        /// </remarks>
        public InteractionFlags RightTrigger { get; set; }

        /// <summary>
        /// Gets or sets the interaction used to move or navigate upward.
        /// </summary>
        /// <remarks>
        /// Typical bindings include W or the Up Arrow key on a keyboard and
        /// D-pad Up on a gamepad.
        /// </remarks>
        public InteractionFlags Up { get; set; }

        /// <summary>
        /// Gets or sets the interaction used to move or navigate downward.
        /// </summary>
        /// <remarks>
        /// Typical bindings include S or the Down Arrow key on a keyboard and
        /// D-pad Down on a gamepad.
        /// </remarks>
        public InteractionFlags Down { get; set; }

        /// <summary>
        /// Gets or sets the interaction used to move or navigate to the left.
        /// </summary>
        /// <remarks>
        /// Typical bindings include A or the Left Arrow key on a keyboard and
        /// D-pad Left on a gamepad.
        /// </remarks>
        public InteractionFlags Left { get; set; }

        /// <summary>
        /// Gets or sets the interaction used to move or navigate to the right.
        /// </summary>
        /// <remarks>
        /// Typical bindings include D or the Right Arrow key on a keyboard and
        /// D-pad Right on a gamepad.
        /// </remarks>
        public InteractionFlags Right { get; set; }

        /// <summary>
        /// Gets or sets the interaction used to activate the left stick action.
        /// </summary>
        /// <remarks>
        /// Typically mapped to pressing the left analog stick on a gamepad.
        /// A keyboard or mouse binding may be assigned by the application when
        /// an equivalent interaction is required.
        /// </remarks>
        public InteractionFlags LeftStick { get; set; }

        /// <summary>
        /// Gets or sets the interaction used to activate the right stick action.
        /// </summary>
        /// <remarks>
        /// Typically mapped to pressing the right analog stick on a gamepad.
        /// A keyboard or mouse binding may be assigned by the application when
        /// an equivalent interaction is required.
        /// </remarks>
        public InteractionFlags RightStick { get; set; }

        /// <summary>
        /// Gets or sets the interaction used to open, start, pause, or confirm
        /// a higher-level tool operation.
        /// </summary>
        /// <remarks>
        /// Typical bindings include Enter or another application-defined key
        /// on a keyboard and the Start or Menu button on a gamepad.
        /// </remarks>
        public InteractionFlags Start { get; set; }

        /// <summary>
        /// Gets or sets the interaction used to return, cancel, or open a
        /// secondary higher-level action.
        /// </summary>
        /// <remarks>
        /// Typical bindings include Escape or Backspace on a keyboard and the
        /// Back, View, or equivalent button on a gamepad.
        /// </remarks>
        public InteractionFlags Back { get; set; }

        /// <summary>
        /// Resets all interaction states to <see cref="InteractionFlags.None"/>.
        /// </summary>
        internal void Reset()
        {
            Primary = InteractionFlags.None;
            Secondary = InteractionFlags.None;
            Tertiary = InteractionFlags.None;
            Quaternary = InteractionFlags.None;

            LeftShoulder = InteractionFlags.None;
            RightShoulder = InteractionFlags.None;
            LeftTrigger = InteractionFlags.None;
            RightTrigger = InteractionFlags.None;

            Up = InteractionFlags.None;
            Down = InteractionFlags.None;
            Left = InteractionFlags.None;
            Right = InteractionFlags.None;

            LeftStick = InteractionFlags.None;
            RightStick = InteractionFlags.None;

            Start = InteractionFlags.None;
            Back = InteractionFlags.None;
        }
    }
}