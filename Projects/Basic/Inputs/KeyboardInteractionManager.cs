using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Sachssoft.Sasogine.Input
{

    public class KeyboardInteractionManager : InputInteractionManager<Keys>
    {
        public KeyboardInteractionManager() : base(new KeyboardStateWrapper())
        {
        }

        public void Update(KeyboardState current_state, TimeSpan elapsed)
        {
            Update(new KeyboardStateWrapper(current_state), elapsed);
        }

        public override void Update(GameTime gameTime)
        {
            var state = Keyboard.GetState();
            Update(state, gameTime.ElapsedGameTime);
        }
    }
}
