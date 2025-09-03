using System;

namespace Sachssoft.Sasogine.Elements;

[Obsolete ("Use Runtime Component")]
public interface IGameObjectElement
{
    bool IsLoaded { get; }

    void Load();

    void Unload();
}
