namespace Sachssoft.Sasogine.Elements;

public interface IGameObjectElement
{
    bool IsLoaded { get; }

    void Load();

    void Unload();
}
