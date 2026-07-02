using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components
{
    public abstract class ResourceComponentBase : IResourceComponent
    {
        public ResourceComponentBase()
        {
        }

        public bool IsLoaded { get; private set; }

        public virtual void Load()
        {
            if (IsLoaded) return;

            IsLoaded = true;
        }

        public virtual void Unload()
        {
            if (!IsLoaded) return;

            IsLoaded = false;
        }
    }
}
