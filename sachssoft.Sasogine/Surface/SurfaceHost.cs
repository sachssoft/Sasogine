using Microsoft.Xna.Framework;
using System;

namespace sachssoft.Sasogine.Surface;

public abstract class SurfaceHost : IDisposable
{
    public virtual void Initialize(Game game) { }

    public virtual ViewBase? View { get; internal protected set; }

    public virtual ISurfaceElement? Root { get; set; }

    //public abstract bool IsMouseOverGUI { get; }

    //public abstract bool IsTouchOverGUI { get; }

    //public abstract bool IsPointOverGUI(Point p);

    public abstract void Render();

    public abstract void Dispose();
}
