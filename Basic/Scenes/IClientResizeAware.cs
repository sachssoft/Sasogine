namespace Sachssoft.Sasogine.Scenes
{
    public interface IClientResizeAware
    {
        bool WasClientResize { get; }

        void OnClientSizeChanged();

        void OnOrientationChanged();
    }
}
