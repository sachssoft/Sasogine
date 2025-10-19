using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Surface;

public abstract class SurfaceHost : IDisposable
{
    public virtual void Initialize(Game game) { }

    public virtual ViewBase? View { get; internal protected set; }

    public virtual ISurfaceElement? Root { get; set; }

    public Game Game { get; internal set; }

    public abstract void Render(GameFrameContext context);

    public abstract void Dispose();
}
