namespace Sachssoft.Sasogine.Experimental.Platform
{
    public abstract class PlatformBase
    {

        public abstract IPlatformInput CreateInput();

        // Windows, Android, MacOs, ...
        public abstract IPlatformWindow CreateWindow(WindowConfiguration configuration);

        // OpenGL, DirectX
        public abstract IPlatformBackend CreateBackend(BackendConfiguration configuration);

    }
}
