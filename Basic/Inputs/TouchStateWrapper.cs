using Microsoft.Xna.Framework.Input.Touch;
using System.Linq;

namespace Sachssoft.Sasogine.Input
{

    public class TouchStateWrapper : IInputState<TouchButton>
    {
        private readonly TouchCollection _touches;

        public TouchStateWrapper(TouchCollection touches)
        {
            _touches = touches;
        }

        public bool IsButtonDown(TouchButton button)
        {
            return button switch
            {
                TouchButton.TouchPressed => _touches.Any(t => t.State == TouchLocationState.Pressed),
                TouchButton.TouchMoved => _touches.Any(t => t.State == TouchLocationState.Moved),
                TouchButton.TouchReleased => _touches.Any(t => t.State == TouchLocationState.Released),
                _ => false
            };
        }

        public bool IsButtonUp(TouchButton button)
        {
            return !IsButtonDown(button);
        }
    }
}