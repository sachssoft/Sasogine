using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Sachssoft.Sasogine.Interactions;

public class TouchInteractionManager : InputInteractionManager<TouchButton>
{
    public TouchInteractionManager(TouchCollection initialTouches)
        : base(new TouchStateWrapper(initialTouches))
    {
    }

    public override void Update(GameTime gameTime)
    //public override void Update(GameContext context)
    {
        var currentTouches = TouchPanel.GetState();
        var currentStateWrapper = new TouchStateWrapper(currentTouches);

        var elapsed = gameTime.ElapsedGameTime;

        Update(currentStateWrapper, elapsed);

        // Hier kann man zusätzlich Touch-spezifische Events (z.B. Position, Gesten) ergänzen
    }
}