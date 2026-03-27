namespace Sachssoft.Sasogine.Components
{
    public abstract class ComponentBase : IResourceComponent
    {
        public ComponentBase()
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
