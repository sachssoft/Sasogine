using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Sachssoft.Sasogine.Interactions;

public class KeyboardInteractionManager : InputInteractionManager<Keys>
{
    public KeyboardInteractionManager() : base(new KeyboardStateWrapper())
    {
    }

    public override void Update(GameFrameContext context)
    {
        var elapsed = context.GameTime.ElapsedGameTime;

        Update(new KeyboardStateWrapper(Keyboard.GetState()), elapsed);
    }

    public void Update(KeyboardState current_state, TimeSpan elapsed)
    {
        Update(new KeyboardStateWrapper(current_state), elapsed);
    }

    public void Update(GameTime game_time)
    {
        var state = Keyboard.GetState();
        Update(state, game_time.ElapsedGameTime);
    }
}
