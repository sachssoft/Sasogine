using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides the base implementation for engine objects driven by a definition.
    ///
    /// A definition contains the static configuration used to initialize the runtime
    /// state of the object. Derived types can apply additional configuration by
    /// overriding <see cref="ConfigureFromDefinition"/>.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// Type of definition used to configure the engine object.
    /// </typeparam>
    public abstract class EngineObject<TDefinition> : EngineObjectBase, IEngineObject
        where TDefinition : class, IDefinition
    {
        private TDefinition? _definition;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineObject{TDefinition}"/> class
        /// with the specified definition.
        /// </summary>
        /// <param name="definition">
        /// Definition used to configure the engine object.
        /// </param>
        public EngineObject(TDefinition definition)
        {
            _definition = definition;
        }

        /// <summary>
        /// Occurs when the identifier of the engine object changes while applying
        /// its definition.
        /// </summary>
        public event EventHandler<EngineObjectChangedEventArgs>? IdChanged;

        /// <summary>
        /// Occurs when the class of the engine object changes while applying
        /// its definition.
        /// </summary>
        public event EventHandler<EngineObjectChangedEventArgs>? ClassChanged;

        /// <summary>
        /// Gets the definition used to configure this engine object.
        ///
        /// The definition represents static configuration data and is typically
        /// used during loading, initialization, serialization, or editor operations.
        /// </summary>
        /// <remarks>
        /// Runtime systems should generally copy frequently accessed values from
        /// the definition into runtime fields during initialization instead of
        /// repeatedly accessing the definition during performance-critical updates.
        ///
        /// If no definition is currently available,
        /// <see cref="EnsureDefinition"/> attempts to resolve one.
        /// </remarks>
        public TDefinition Definition
        {
            get
            {
                EnsureDefinition();
                return _definition;
            }
        }

        IDefinition? IEngineObject.Definition => Definition;

        /// <summary>
        /// Loads the engine object and applies its definition.
        ///
        /// Calling this method when the object is already loaded has no effect.
        /// </summary>
        public override void Load()
        {
            if (IsLoaded)
                return;

            if (Definition != null)
            {
                ConfigureFromDefinitionInternal();
                ConfigureFromDefinition();
            }

            IsLoaded = true;
        }

        /// <summary>
        /// Asynchronously loads the engine object and applies its definition.
        ///
        /// Calling this method when the object is already loaded has no effect.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous load operation.
        /// </returns>
        public override async Task LoadAsync()
        {
            if (IsLoaded)
            {
                await Task.CompletedTask;
                return;
            }

            if (Definition != null)
            {
                ConfigureFromDefinitionInternal();
                ConfigureFromDefinition();
            }

            IsLoaded = true;

            await Task.CompletedTask;
            return;
        }

        /// <summary>
        /// Unloads the engine object.
        ///
        /// Calling this method when the object is not loaded has no effect.
        /// </summary>
        public override void Unload()
        {
            if (!IsLoaded)
                return;

            IsLoaded = false;
        }

        /// <summary>
        /// Ensures that a definition is available for this engine object.
        ///
        /// If no definition is currently assigned, <see cref="ResolveDefinition"/>
        /// is invoked to resolve one.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ResolveDefinition"/> does not provide a definition.
        /// </exception>
        [MemberNotNull(nameof(_definition))]
        public void EnsureDefinition()
        {
            if (_definition != null)
                return;

            _definition = ResolveDefinition()
                ?? throw new InvalidOperationException(
                    $"ResolveDefinition returned null for {GetType().Name}");
        }

        /// <summary>
        /// Resolves the definition used by this engine object when no definition
        /// is currently available.
        /// </summary>
        /// <returns>
        /// The resolved definition.
        /// </returns>
        /// <remarks>
        /// Derived classes can override this method to provide definitions from
        /// alternative sources such as asset stores, registries, or runtime providers.
        /// </remarks>
        protected virtual TDefinition ResolveDefinition()
        {
            return _definition!;
        }

        /// <summary>
        /// Applies definition-specific configuration to the engine object.
        ///
        /// Override this method in derived classes to copy configuration values
        /// from <see cref="Definition"/> into runtime fields.
        /// </summary>
        protected virtual void ConfigureFromDefinition()
        {
        }

        /// <summary>
        /// Applies common engine object properties from the definition and raises
        /// the corresponding change events when necessary.
        /// </summary>
        private void ConfigureFromDefinitionInternal()
        {
            if (Definition is not IEngineObjectDefinition eod)
                return;

            if (!Equals(Id, eod.Id))
            {
                var oldId = Id;
                Id = eod.Id;

                IdChanged?.Invoke(
                    this,
                    new EngineObjectChangedEventArgs(
                        oldId,
                        Id,
                        Class,
                        Class));
            }

            if (!Equals(Class, eod.Class))
            {
                var oldClass = Class;
                Class = eod.Class;

                ClassChanged?.Invoke(
                    this,
                    new EngineObjectChangedEventArgs(
                        Id,
                        Id,
                        oldClass,
                        Class));
            }
        }
    }
}