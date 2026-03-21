using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasogine.Engine;

namespace Sachssoft.Sasogine.Inspection
{
    public class GameObject : NotifyingElement, IDrawableRuntimeComponent, IResourceComponent
    {
        private bool _isLoaded;

        public bool IsLoaded => _isLoaded;

        public virtual void Load()
        {
            if (_isLoaded)
                return;

            _isLoaded = true;
        }

        public virtual void Unload()
        {
            if (!_isLoaded)
                return;

            _isLoaded = false;
        }

        public virtual void Update(RuntimeContext context)
        {
        }

        public virtual void Draw(RuntimeViewportContext context)
        {
        }
    }
}
