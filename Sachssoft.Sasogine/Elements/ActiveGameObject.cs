using Sachssoft.Observables;

namespace Sachssoft.Sasogine.Elements
{
    public class ActiveGameObject : NotifyingElement, IActiveGameObjectElement
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

        public virtual void Update(GameContext context)
        {
        }

        public virtual void Draw(GameContext context)
        {
        }
    }
}
