namespace Sachssoft.Sasogine.Scenes
{
    // UI Logik
    public interface ISceneWithPresentation : IScene
    {
        IPresentation Presentation { get; }
    }
}
