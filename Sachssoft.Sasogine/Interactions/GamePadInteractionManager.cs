using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Interactions;

public class GamePadInteractionManager : InputInteractionManager<Buttons>
{
    private readonly PlayerIndex _player_index;

    public GamePadInteractionManager(PlayerIndex player_index, GamePadState initialState)
        : base(new GamePadStateWrapper(initialState))
    {
        _player_index = player_index;
    }

    public override void Update(GameFrameContext context)
    {     
        var elapsed = context.GameTime.ElapsedGameTime;
        Update(new GamePadStateWrapper(GamePad.GetState(_player_index)), elapsed);
    }

    public void Update(GamePadState current_state, TimeSpan elapsed)
    {
        Update(new GamePadStateWrapper(current_state), elapsed);
    }

    public void Update(GameTime game_time)
    {
        var state = GamePad.GetState(_player_index);
        Update(state, game_time.ElapsedGameTime);
    }
}