
namespace Sachssoft.Sasogine;

public abstract class GameApplication<TAssetManager> : GameApplication where TAssetManager : GameAssetManager
{
    public GameApplication(params string[] args) : base(args)
    {
    }

    public static new GameApplication<TAssetManager> Current
    {
        get => (GameApplication<TAssetManager>)IGameApplication.Current;
    }

    public new TAssetManager Assets => (TAssetManager)_assets;

    protected abstract TAssetManager CreateAssets();

    protected sealed override GameAssetManager? AssetsOverride() => CreateAssets();

    protected override void OnLoad()
    {
        base.OnLoad();
    }

}