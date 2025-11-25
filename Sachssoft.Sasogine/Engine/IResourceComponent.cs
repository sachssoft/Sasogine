namespace Sachssoft.Sasogine.Engine
{
    public interface IResourceComponent : IComponent
    {
        bool IsLoaded { get; }

        void Load();

        void Unload();
    }
}
