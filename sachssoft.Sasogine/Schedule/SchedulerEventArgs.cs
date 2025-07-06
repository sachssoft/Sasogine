using Microsoft.Xna.Framework;
using System.ComponentModel;

namespace sachssoft.Sasogine.Schedule;

public class SchedulerEventArgs : CancelEventArgs
{
    public SchedulerEventArgs(GameTime game_time)
    {
        GameTime = game_time;
    }

    public GameTime GameTime { get; }

}
