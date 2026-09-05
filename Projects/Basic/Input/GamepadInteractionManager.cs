using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Manages gamepad input interactions for a specific player.
    /// </summary>
    public class GamepadInteractionManager : InputInteractionManager<Buttons>
    {
        private readonly PlayerIndex _playerIndex;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="GamepadInteractionManager"/> class.
        /// </summary>
        /// <param name="playerIndex">
        /// The player index associated with the gamepad.
        /// </param>
        /// <param name="initialState">
        /// The initial gamepad state.
        /// </param>
        public GamepadInteractionManager(
            PlayerIndex playerIndex,
            GamePadState initialState)
            : base(new GamepadStateWrapper(initialState))
        {
            _playerIndex = playerIndex;
        }

        /// <summary>
        /// Updates the gamepad interactions using the current scene update context.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        public override void Update(
            SceneUpdateContext context)
        {
            var state =
                GamePad.GetState(_playerIndex);

            UpdateState(
                new GamepadStateWrapper(state),
                context.GameTime.ElapsedGameTime);
        }
    }
}