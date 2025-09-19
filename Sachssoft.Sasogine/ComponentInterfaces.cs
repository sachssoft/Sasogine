using Sachssoft.Sasogine.Surface;

namespace Sachssoft.Sasogine
{
    public interface IComponent
    {
    }

    public interface IRuntimeComponent : IComponent
    {
        void Update(GameFrameContext context);

    }

    public interface IDrawableRuntimeComponent : IRuntimeComponent
    {
        void Draw(GameFrameContext context);
    }

    public interface IResourceComponent : IComponent
    {
        bool IsLoaded { get; }

        void Load(GameBaseContext context);

        void Unload();
    }
}
