

using Sachssoft.Sasogine.Resources;

namespace Sachssoft.Sasogine;

public abstract class GameApplication<TResourceManager> : GameApplication where TResourceManager : GameResourceManager
{
    public GameApplication(params string[] args) : base(args)
    {
    }

    public static new GameApplication<TResourceManager> Current
    {
        get => (GameApplication<TResourceManager>)IGameApplication.Current;
    }

    public new TResourceManager Resources => (TResourceManager)_resourceManager;

    protected abstract TResourceManager CreateResources();

    protected sealed override GameResourceManager? ResourcesOverride() => CreateResources();

}