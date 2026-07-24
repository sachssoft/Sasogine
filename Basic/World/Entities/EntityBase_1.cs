using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Provides a base implementation for entities that are defined by a data definition
    /// and composed of updateable and drawable components.
    /// Manages entity lifecycle, component handling, state changes, updating, and rendering.
    /// </summary>
    public abstract class EntityBase<TDefinition> : EngineObject<TDefinition>, IEntity, IComponentProvider, IUpdatableComponent, IDrawableComponent
        where TDefinition : class, IEntityDefinition
    {
        private readonly ComponentCollection _components;
        private EntityIntegrity _status = EntityIntegrity.Intact;
        private ActivityState _activityState = ActivityState.Idle;

        /// <summary>
        /// Occurs when the entity has been loaded.
        /// </summary>
        public event EventHandler? Loaded;

        /// <summary>
        /// Occurs when the entity has been unloaded.
        /// </summary>
        public event EventHandler? Unloaded;

        /// <summary>
        /// Occurs when the entity integrity state changes.
        /// </summary>
        public event EventHandler? StatusChanged;

        /// <summary>
        /// Occurs when the entity activity state changes.
        /// </summary>
        public event EventHandler? ActivityStateChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBase{TDefinition}"/> class.
        /// </summary>
        /// <param name="definition">
        /// The definition data used to initialize this entity.
        /// </param>
        protected EntityBase(TDefinition definition) : base(definition)
        {
            _components = new ComponentCollection();
        }

        /// <summary>
        /// Gets or sets whether the entity participates in updates.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether the entity is rendered.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets the current integrity state of the entity.
        /// </summary>
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

        /// <summary>
        /// Gets the current activity state of the entity.
        /// </summary>
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

        /// <inheritdoc/>
        public override void Load()
        {
            //if (_registry == null)
            //    throw new InvalidOperationException($"Node {GetType().Name} has not been initialized. Call Initialize() before Load().");

            base.Load();
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public override Task LoadAsync()
        {
            //if (_registry == null)
            //    throw new InvalidOperationException($"Node {GetType().Name} has not been initialized. Call Initialize() before LoadAsync().");

            var task = base.LoadAsync();
            Loaded?.Invoke(this, EventArgs.Empty);
            return task;
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            base.Unload();
            Unloaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Updates all updateable components attached to this entity.
        /// </summary>
        /// <param name="context">
        /// Provides scene update information.
        /// </param>
        public virtual void Update(SceneUpdateContext context)
        {
            _components.UpdateForEach(context);
        }

        /// <summary>
        /// Draws all drawable components attached to this entity.
        /// </summary>
        /// <param name="context">
        /// Provides scene rendering information.
        /// </param>
        public virtual void Draw(SceneDrawContext context)
        {
            _components.DrawForEach(context);
        }

        /// <summary>
        /// Attempts to retrieve a component of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The component type to retrieve.
        /// </typeparam>
        /// <param name="component">
        /// Receives the component instance if found.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a matching component exists; otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGetComponent<T>([MaybeNullWhen(false)] out T component) where T : class, IComponent
        {
            return _components.TryGet<T>(out component);
        }

        /// <summary>
        /// Gets the collection of components attached to this entity.
        /// </summary>
        protected ComponentCollection Components => _components;
    }
}