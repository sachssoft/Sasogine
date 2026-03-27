using Sachssoft.Sasogine.Common;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    public abstract class NodeBase<TDefinition> : ElementBase<TDefinition>, INode
        where TDefinition : class, INodeDefinition
    {
        private NodeDefinitionRegistry _registry = null!;
        private TDefinition _definition = null!;
        private NodeStatus _status = NodeStatus.Intact;
        private ActivityState _activityState = ActivityState.Ready;

        public event EventHandler? Loaded;
        public event EventHandler? Unloaded;
        public event EventHandler? StatusChanged;
        public event EventHandler? ActivityStateChanged;

        protected NodeBase() { }

        public override TDefinition Definition => _definition;

        public NodeStatus Status
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
        /// Initializes the component with a registry and creates its definition.
        /// </summary>
        /// <param name="registry">The registry from which the definition will be created.</param>
        /// <exception cref="ArgumentNullException">Thrown if the registry is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the definition could not be created.</exception>
        public void Initialize(NodeDefinitionRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry), "Registry cannot be null.");

            _definition = (TDefinition)_registry.Create(GetType(), typeof(TDefinition))
                         ?? throw new InvalidOperationException($"Definition for {typeof(TDefinition).Name} could not be created.");
        }

        /// <summary>
        /// Loads the component and applies the definition.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if Initialize() has not been called.</exception>
        public override void Load()
        {
            if (_registry == null)
                throw new InvalidOperationException($"Node {GetType().Name} has not been initialized. Call Initialize() before Load().");

            base.Load();
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        public override Task LoadAsync()
        {
            if (_registry == null)
                throw new InvalidOperationException($"Node {GetType().Name} has not been initialized. Call Initialize() before LoadAsync().");

            var task = base.LoadAsync();
            Loaded?.Invoke(this, EventArgs.Empty);
            return task;
        }

        public override void Unload()
        {
            base.Unload();
            Unloaded?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Update(GameContext gameContext)
        {
            // ...
        }
    }
}