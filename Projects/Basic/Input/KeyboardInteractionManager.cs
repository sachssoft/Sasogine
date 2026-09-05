using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Manages keyboard input interactions.
    /// </summary>
    public class KeyboardInteractionManager : InputInteractionManager<Keys>
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="KeyboardInteractionManager"/> class.
        /// </summary>
        public KeyboardInteractionManager()
            : base(new KeyboardStateWrapper())
        {
        }

        /// <summary>
        /// Updates the keyboard interactions using the current scene update context.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        public override void Update(
            SceneUpdateContext context)
        {
            var state =
                Keyboard.GetState();

            UpdateState(
                new KeyboardStateWrapper(state),
                context.GameTime.ElapsedGameTime);
        }
    }
}