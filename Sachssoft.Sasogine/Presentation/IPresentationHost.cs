using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Presentation;

public interface IPresentationHost : IDisposable
{
    void Initialize(Game game) { }

    SceneBase? Scene { get; set; }

    IPresentationHostElement? RootElement { get; set; }

    Game Game { get; }

    void Render(PresentationContext context);
}