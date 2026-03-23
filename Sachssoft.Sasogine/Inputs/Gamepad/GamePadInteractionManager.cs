using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Interactions;

public class GamePadInteractionManager : InputInteractionManager<Buttons>
{
    private readonly PlayerIndex _playerIndex;

    public GamePadInteractionManager(PlayerIndex playerIndex, GamePadState initialState)
        : base(new GamePadStateWrapper(initialState))
    {
        _playerIndex = playerIndex;
    }

    public void Update(GamePadState currentState, TimeSpan elapsed)
    {
        Update(new GamePadStateWrapper(currentState), elapsed);
    }

    public void Update(TimeSpan elapsed)
    {
        Update(new GamePadStateWrapper(GamePad.GetState(_playerIndex)), elapsed);
    }

    public override void Update(GameTime gameTime)
    {
        var state = GamePad.GetState(_playerIndex);
        Update(state, gameTime.ElapsedGameTime);
    }
}