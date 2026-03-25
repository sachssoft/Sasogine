namespace Sachssoft.Sasogine.Scenes
{
    public interface ISceneWithRuntime : IScene
    {
        RuntimeBase Runtime { get; }
    }
}
