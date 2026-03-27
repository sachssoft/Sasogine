namespace Sachssoft.Sasogine.Scenes
{
    public interface IScene
    {
        bool KeepAlive { get; }

        void Load();
        void Unload();

        void OnEnter(SceneContext context);   // Scene wird aktiv
        void OnExit();    // Scene wird verlassen

        void Update(GameContext context);
        void Draw(GameContext context);
    }
}
