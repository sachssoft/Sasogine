/* 
 * © 2024 Tobias Sachs
 * GameContext
 * 11.07.2024 
 * Update: 23.05.2025
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Diagnostics;
using Sachssoft.Sasogine.Interactions;
using Sachssoft.Sasogine.Surface;
using System;

namespace Sachssoft.Sasogine;

public class GameContext : IDisposable
{
    public GameContext(GameContext self)
        : this(self.View, /*self.InputEvents,*/ self.GameTime)
    {
    }

    public GameContext(ViewBase view, /*InputEvents input_events,*/ GameTime time)
    {
        IsUIVisibled = true;
        View = view;
        Runtime = view.Runtime;
        GameTime = time;
        FrameCounter = new();
        //InputEvents = input_events;

        FrameCounter.Update(ElapsedTimeInSeconds);
    }

    public IMyGameApp CurrentApp => IMyGameApp.Current;

    public GraphicsDevice GraphicsDevice => IMyGameApp.Current.GraphicsDevice;

    public ViewBase View { get; }

    public RuntimeBase? Runtime { get; }

    public GameTime GameTime { get; }

    public float ElapsedTimeInSeconds => (float)GameTime.ElapsedGameTime.TotalSeconds;

    public float ElapsedTimeInMilliseconds => (float)GameTime.ElapsedGameTime.TotalMilliseconds;

    public FrameCounter FrameCounter { get; }

    public TimeSpan BenchmarkTime { get; }

    public bool IsUIVisibled { get; set; }

    //public InputEvents InputEvents { get; }

    public object? Parameter { get; set; }

    public void Dispose()
    {
    }
}
