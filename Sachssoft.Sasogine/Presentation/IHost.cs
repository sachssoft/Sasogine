using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Presentation;

public interface IHost : IDisposable
{
    void Initialize(Game game) { }

    SceneBase? Scene { get; set; }

    IHostElement? RootElement { get; set; }

    Game Game { get; }

    void Render(GameFrameContext context);
}


//public abstract class Host : IDisposable
//{
//    public virtual void Initialize(Game game) { }

//    public virtual SceneBase? Scene { get; internal protected set; }

//    public virtual IHostElement? RootElement { get; set; }

//    [AllowNull]
//    public Game Game { get; internal set; }

//    public abstract void Render(GameFrameContext context);

//    public abstract void Dispose();
//}
