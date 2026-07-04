using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common.Models
{
    public abstract class EngineObjectBase : IEngineReferenceable
    {
        private string? _id;
        private string? _class;
        private bool _isFrozen = false;

        internal EngineObjectBase() { }

        /// <summary>
        /// Indicates whether this element has been loaded.
        /// </summary>
        public bool IsLoaded { get; private protected set; }

        /// <summary>
        /// Unique identifier for this element.
        /// Set automatically from the Definition. Read-only at runtime.
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
        /// Classification or category of this element.
        /// Set automatically from the Definition. Read-only at runtime.
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
        /// Optional user-defined context associated with this element.
        /// Can hold any object, e.g., editor metadata, runtime state, or scripting data.
        /// </summary>
        public object? DataContext { get; set; }

        public bool IsFrozen => _isFrozen;

        /// <summary>
        /// Loads the element and applies the definition.
        /// Hooks up definition change events.
        /// </summary>
        public abstract void Load();

        public abstract Task LoadAsync();

        /// <summary>
        /// Unloads the element and detaches definition change events.
        /// </summary>
        public abstract void Unload();

        public void Reload()
        {
            Unload();
            Load();
        }

        public async Task ReloadAsync()
        {
            Unload();
            await LoadAsync();
        }

        protected void Freeze()
        {
            if (_isFrozen)
                throw new InvalidOperationException("Object is already frozen.");

            _isFrozen = true;
        }

        protected void ThrowIfFrozen()
        {
            if (_isFrozen)
                throw new InvalidOperationException("Object is frozen and immutable.");
        }
    }
}
