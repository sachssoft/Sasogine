using Sachssoft.Sasogine;

namespace Sachssoft.VarietyGolf.Core
{
    public abstract class PhysicContextBase<T>
    {
        private readonly T _world;

        public PhysicContextBase()
        {
            _world = GetWorld();
        }

        public bool IsPreviewVisibility { get; set; }

        public T World => _world;

        protected abstract T GetWorld();

        public virtual void Update()
        {
        }

        public virtual void Draw(GameContext context)
        {
        }
    }
}
