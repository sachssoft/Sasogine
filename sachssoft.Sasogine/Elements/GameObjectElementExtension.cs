using System.Collections.Generic;

namespace sachssoft.Sasogine.Elements;

public static class GameObjectElementExtension
{
    public static void Load(this IGameObjectElement elem) => elem.Load();

    public static void Unload(this IGameObjectElement elem) => elem.Unload();

    public static void LoadAll(this IEnumerable<IGameObjectElement> elems)
    {
        foreach (var elem in elems) elem.Load();
    }

    public static void UnloadAll(this IEnumerable<IGameObjectElement> elems)
    {
        foreach (var elem in elems) elem.Unload();
    }

    public static void Update(this IActiveGameObjectElement elem, GameContext context) => elem.Update(context);

    public static void Draw(this IActiveGameObjectElement elem, GameContext context) => elem.Draw(context);

    public static void UpdateAll(this IEnumerable<IActiveGameObjectElement> elems, GameContext context)
    {
        foreach (var elem in elems) elem.Update(context);
    }

    public static void DrawAll(this IEnumerable<IActiveGameObjectElement> elems, GameContext context)
    {
        foreach (var elem in elems) elem.Draw(context);
    }
}
