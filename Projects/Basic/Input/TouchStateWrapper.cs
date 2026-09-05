using Microsoft.Xna.Framework.Input.Touch;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Provides a touch input state backed by a MonoGame
    /// <see cref="TouchCollection"/>.
    /// </summary>
    public sealed class TouchStateWrapper : IInputState<TouchButton>
    {
        private readonly TouchCollection _touches;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchStateWrapper"/> class.
        /// </summary>
        /// <param name="touches">The touch collection to wrap.</param>
        public TouchStateWrapper(TouchCollection touches)
        {
            _touches = touches;
        }

        /// <inheritdoc />
        public bool IsButtonDown(TouchButton button)
        {
            var state = button switch
            {
                TouchButton.Pressed => TouchLocationState.Pressed,
                TouchButton.Moved => TouchLocationState.Moved,
                TouchButton.Released => TouchLocationState.Released,
                _ => TouchLocationState.Invalid
            };

            if (state == TouchLocationState.Invalid)
                return false;

            for (int i = 0; i < _touches.Count; i++)
            {
                if (_touches[i].State == state)
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool IsButtonUp(TouchButton button)
        {
            return !IsButtonDown(button);
        }
    }
}