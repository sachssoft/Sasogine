using Sachssoft.Sasogine.Experimental.Graphics;

namespace Sachssoft.Sasogine.Experimental.Platform
{
    public interface IPlatformBackend : IDisposable
    {
        void Initialize(IPlatformWindow window);

        void ApplyChange(BackendConfiguration configuration);

        void Present(); // SwapBuffers oder Present

        void Clear(Color color);

        int DisplayWidth { get; }

        int DisplayHeight { get; }
    }
}
