using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Provides a keyboard input state backed by a MonoGame
    /// <see cref="KeyboardState"/>.
    /// </summary>
    public sealed class KeyboardStateWrapper : IInputState<Keys>
    {
        private readonly KeyboardState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardStateWrapper"/> class
        /// with an empty keyboard state.
        /// </summary>
        public KeyboardStateWrapper()
            : this(new KeyboardState())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardStateWrapper"/> class.
        /// </summary>
        /// <param name="state">The keyboard state to wrap.</param>
        public KeyboardStateWrapper(KeyboardState state)
        {
            _state = state;
        }

        /// <inheritdoc />
        public bool IsButtonDown(Keys button) => _state.IsKeyDown(button);

        /// <inheritdoc />
        public bool IsButtonUp(Keys button) => _state.IsKeyUp(button);
    }
}