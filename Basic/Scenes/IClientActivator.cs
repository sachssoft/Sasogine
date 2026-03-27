namespace Sachssoft.Sasogine.Scenes
{
    public interface IClientActivator
    {
        bool WasActivated { get; }

        bool WasDeactivated { get; }

        void OnClientActivate();

        void OnClientDeactivate();
    }
}
