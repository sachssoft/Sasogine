using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components;
using System;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.World
{
    public abstract class EntityBase<TDefinition> : EngineObject<TDefinition>, IEntity, IComponentProvider, IUpdatableComponent, IDrawableComponent
        where TDefinition : class, IEntityDefinition
    {
        private readonly ComponentCollection _components;
        private EntityIntegrity _status = EntityIntegrity.Intact;
        private ActivityState _activityState = ActivityState.Idle;

        public event EventHandler? Loaded;
        public event EventHandler? Unloaded;
        public event EventHandler? StatusChanged;
        public event EventHandler? ActivityStateChanged;

        public bool IsEnabled { get; set; }

        public bool IsVisible { get; set; }

        protected EntityBase()
        {
            _components = new ComponentCollection();
        }

        //public override TDefinition Definition => _definition;

        public EntityIntegrity Integrity
        {
            get => _status;
            protected set
            {
                if (Equals(_status, value)) return;
                _status = value;
                StatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public ActivityState ActivityState
        {
            get => _activityState;
            protected set
            {
                if (Equals(_activityState, value)) return;
                _activityState = value;
                ActivityStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Loads the component and applies the definition.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if Initialize() has not been called.</exception>
        public override void Load()
        {
            //if (_registry == null)
            //    throw new InvalidOperationException($"Node {GetType().Name} has not been initialized. Call Initialize() before Load().");

            base.Load();
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        public override Task LoadAsync()
        {
            //if (_registry == null)
            //    throw new InvalidOperationException($"Node {GetType().Name} has not been initialized. Call Initialize() before LoadAsync().");

            var task = base.LoadAsync();
            Loaded?.Invoke(this, EventArgs.Empty);
            return task;
        }

        public override void Unload()
        {
            base.Unload();
            Unloaded?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Update(SceneUpdateContext context)
        {
            _components.UpdateForEach(context);
        }

        public virtual void Draw(SceneDrawContext context)
        {
            _components.DrawForEach(context);
        }

        public bool TryGetComponent<T>([MaybeNullWhen(false)] out T component) where T : class, IComponent
        {
            return _components.TryGet<T>(out component);
        }

        protected ComponentCollection Components => _components;
    }
}