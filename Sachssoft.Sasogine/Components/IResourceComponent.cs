namespace Sachssoft.Sasogine.Components
{
    public interface IResourceComponent : IComponent
    {
        bool IsLoaded { get; }

        void Load();

        void Unload();
    }
}
