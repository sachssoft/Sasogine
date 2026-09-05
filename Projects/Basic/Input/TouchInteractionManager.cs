using Microsoft.Xna.Framework.Input.Touch;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Manages touch input interactions.
    /// </summary>
    public class TouchInteractionManager : InputInteractionManager<TouchButton>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TouchInteractionManager"/> class.
        /// </summary>
        /// <param name="initialTouches">The initial touch state.</param>
        public TouchInteractionManager(TouchCollection initialTouches)
            : base(new TouchStateWrapper(initialTouches))
        {
        }

        /// <summary>
        /// Updates the touch interactions using the current scene update context.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        public override void Update(SceneUpdateContext context)
        {
            var currentTouches = TouchPanel.GetState();

            UpdateState(
                new TouchStateWrapper(currentTouches),
                context.GameTime.ElapsedGameTime);
        }
    }
}