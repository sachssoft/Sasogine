namespace Sachssoft.Sasogine.Scenes
{
    // UI Logik
    public interface ISceneWithPresentation : IScene
    {
        IPresentationHost Presentation { get; }
    }
}
