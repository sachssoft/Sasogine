using Sachssoft.Sasogine.Common;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Components
{
    public abstract class ComponentBase<TDefinition> : ElementBase<TDefinition>, IResourceComponent
        where TDefinition : class, IComponentDefinition
    {
        private ComponentDefinitionRegistry _registry = null!;
        private TDefinition _definition = null!;

        protected ComponentBase() { }

        public override TDefinition Definition => _definition;

        /// <summary>
        /// Initializes the component with a registry and creates its definition.
        /// </summary>
        /// <param name="registry">The registry from which the definition will be created.</param>
        /// <exception cref="ArgumentNullException">Thrown if the registry is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the definition could not be created.</exception>
        public void Initialize(ComponentDefinitionRegistry registry)
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
                throw new InvalidOperationException($"Component {GetType().Name} has not been initialized. Call Initialize() before Load().");

            base.Load();
        }

        public override Task LoadAsync()
        {
            if (_registry == null)
                throw new InvalidOperationException($"Component {GetType().Name} has not been initialized. Call Initialize() before LoadAsync().");

            return base.LoadAsync();
        }
    }
}