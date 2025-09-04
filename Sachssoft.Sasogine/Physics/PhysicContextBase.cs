using Sachssoft.Sasogine;

namespace Sachssoft.VarietyGolf.Core
{
    public abstract class PhysicContextBase<T>
    {
        private readonly T _world;

        public PhysicContextBase()
        {
            _world = BuildWorld();
        }

        public bool IsDebugEnabled { get; set; }

        public T World => _world;

        protected abstract T BuildWorld();

        public virtual void Update(GameContext context)
        {
        }

        public virtual void Draw(GameContext context)
        {
        }
    }
}
