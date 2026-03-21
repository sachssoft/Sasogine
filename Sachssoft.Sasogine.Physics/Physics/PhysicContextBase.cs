using Sachssoft.Sasogine.Engine;

namespace Sachssoft.Sasogine.Phyiscs
{
    public abstract class PhysicContextBase<T> : IPhysicsContext
    {
        private readonly T _world;

        public PhysicContextBase()
        {
            _world = BuildWorld();
        }

        public bool IsDebugEnabled { get; set; }

        public T World => _world;

        object IPhysicsContext.PhysicsWorld => _world!;

        protected abstract T BuildWorld();

        public virtual void Update(RuntimeContext context)
        {
        }

        public virtual void Draw(RuntimeViewportContext context)
        {
        }
    }
}
