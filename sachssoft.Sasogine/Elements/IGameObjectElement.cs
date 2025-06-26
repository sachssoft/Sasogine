namespace sachssoft.Sasogine.Elements;

public interface IGameObjectElement
{
    bool IsLoaded { get; }

    void Load();

    void Unload();
}
