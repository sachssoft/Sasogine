using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides the non-generic base implementation for engine objects.
    ///
    /// Defines common runtime state such as loading, identification,
    /// classification, user-defined context, and object freezing.
    /// </summary>
    public abstract class EngineObjectBase : IEngineReferenceable
    {
        private string? _id;
        private string? _class;
        private bool _isFrozen;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineObjectBase"/> class.
        /// </summary>
        internal EngineObjectBase()
        {
        }

        /// <summary>
        /// Gets whether this engine object has been loaded.
        /// </summary>
        public bool IsLoaded { get; private protected set; }

        /// <summary>
        /// Gets the unique identifier of this engine object.
        ///
        /// The identifier is typically assigned from the object's definition
        /// during loading and cannot be changed publicly at runtime.
        /// </summary>
        public string? Id
        {
            get => _id;
            private protected set
            {
                ThrowIfFrozen();
                _id = value;
            }
        }

        /// <summary>
        /// Gets the classification or category of this engine object.
        ///
        /// The class is typically assigned from the object's definition
        /// during loading and cannot be changed publicly at runtime.
        /// </summary>
        public string? Class
        {
            get => _class;
            private protected set
            {
                ThrowIfFrozen();
                _class = value;
            }
        }

        /// <summary>
        /// Gets or sets optional user-defined data associated with this engine object.
        ///
        /// This can be used to store editor metadata, runtime state,
        /// scripting data, or other application-specific information.
        /// </summary>
        public object? DataContext { get; set; }

        /// <summary>
        /// Gets whether this engine object has been frozen.
        /// </summary>
        public bool IsFrozen => _isFrozen;

        /// <summary>
        /// Loads and initializes the engine object.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Asynchronously loads and initializes the engine object.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous load operation.
        /// </returns>
        public abstract Task LoadAsync();

        /// <summary>
        /// Unloads the engine object and releases or resets load-related state.
        /// </summary>
        public abstract void Unload();

        /// <summary>
        /// Reloads the engine object by unloading and loading it again.
        /// </summary>
        public void Reload()
        {
            Unload();
            Load();
        }

        /// <summary>
        /// Asynchronously reloads the engine object by unloading it and
        /// asynchronously loading it again.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous reload operation.
        /// </returns>
        public async Task ReloadAsync()
        {
            Unload();
            await LoadAsync();
        }

        /// <summary>
        /// Freezes this engine object and prevents further modifications
        /// to protected immutable state.
        ///
        /// Calling this method more than once has no effect.
        /// </summary>
        protected void Freeze()
        {
            if (_isFrozen)
                return;

            _isFrozen = true;
        }

        /// <summary>
        /// Throws an exception when this engine object is frozen.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the object is frozen.
        /// </exception>
        protected void ThrowIfFrozen()
        {
            if (_isFrozen)
                throw new InvalidOperationException(
                    "Object is frozen and immutable.");
        }
    }
}