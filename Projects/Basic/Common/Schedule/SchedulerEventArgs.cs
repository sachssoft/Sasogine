using Microsoft.Xna.Framework;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Common.Schedule;

public class SchedulerEventArgs : CancelEventArgs
{
    public SchedulerEventArgs(GameTime game_time)
    {
        GameTime = game_time;
    }

    public GameTime GameTime { get; }

}
