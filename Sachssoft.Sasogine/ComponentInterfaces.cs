namespace Sachssoft.Sasogine
{
    public interface IComponent
    {
    }

    public interface IRuntimeComponent : IComponent
    {
        void Update(GameContext context);

    }
    public interface IDrawableRuntimeComponent : IRuntimeComponent
    {
        void Draw(GameContext context);
    }

    public interface IMultiScreenDrawableRuntimeComponent : IRuntimeComponent
    {
        void Draw(MultiScreenGameContext context);
    }

    public interface IResourceComponent : IComponent
    {
        bool IsLoaded { get; }

        void Load();

        void Unload();
    }
}
