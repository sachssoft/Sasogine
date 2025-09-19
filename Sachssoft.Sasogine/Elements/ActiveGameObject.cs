using Sachssoft.Observables;
using Sachssoft.Sasogine.Surface;

namespace Sachssoft.Sasogine.Elements
{
    public class ActiveGameObject : NotifyingElement, IDrawableRuntimeComponent, IResourceComponent
    {
        private bool _isLoaded;

        public bool IsLoaded => _isLoaded;

        public virtual void Load(ViewContext context)
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

        public virtual void Update(GameContext context)
        {
        }

        public virtual void Draw(GameContext context)
        {
        }
    }
}
