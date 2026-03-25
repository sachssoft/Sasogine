using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Scenes;

// Speziell für UI
public interface IPresentationHost : IDisposable
{
    void Load();

    void Unload();

    void Update(PresentationContext context);

    void Draw(PresentationContext context);

    IScene Scene { get; }

    bool IsVisible { get; }
}