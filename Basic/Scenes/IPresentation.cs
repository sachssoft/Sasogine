

namespace Sachssoft.Sasogine.Scenes;

// Speziell für UI
public interface IPresentation
{
    void Load();

    void Unload();

    void Update(PresentationContext context);

    void Draw(PresentationContext context);

    IScene Scene { get; }
}