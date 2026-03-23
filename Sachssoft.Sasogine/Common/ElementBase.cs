using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Base class for all framework elements that are driven by a definition.
    /// Can be a Component, Asset, Scene element, etc.
    /// </summary>
    /// <typeparam name="TDefinition">Type of definition that drives this element.</typeparam>
    public abstract class ElementBase<TDefinition>
        where TDefinition : class, IElementDefinition
    {
        private bool _isLoaded;

        /// <summary>
        /// The definition instance that configures this element.
        /// Must be provided at construction time to ensure immutability at runtime.
        /// </summary>
        [AllowNull]
        public virtual TDefinition Definition { get; }

        /// <summary>
        /// Indicates whether this element has been loaded.
        /// </summary>
        public virtual bool IsLoaded => _isLoaded;

        /// <summary>
        /// Unique identifier for this element.
        /// Set automatically from the Definition. Read-only at runtime.
        /// </summary>
        public string? Id { get; private set; }

        /// <summary>
        /// Classification or category of this element.
        /// Set automatically from the Definition. Read-only at runtime.
        /// </summary>
        public string? Class { get; private set; }

        /// <summary>
        /// Optional user-defined context associated with this element.
        /// Can hold any object, e.g., editor metadata, runtime state, or scripting data.
        /// </summary>
        public object? DataContext { get; set; }

        /// <summary>
        /// Loads the element and applies the definition.
        /// Hooks up definition change events.
        /// </summary>
        public virtual void Load()
        {
            if (IsLoaded)
                return;

            if (Definition != null)
            {
                // Apply all properties from the definition
                ApplyDefinition();

                // Subscribe to definition changes
                Definition.Changed += Definition_Changed;
            }

            _isLoaded = true;
        }

        public virtual Task LoadAsync()
        {
            if (IsLoaded)
                return Task.CompletedTask;

            if (Definition != null)
            {
                // Apply all properties from the definition
                ApplyDefinition();

                // Subscribe to definition changes
                Definition.Changed += Definition_Changed;
            }

            _isLoaded = true; 
            return Task.CompletedTask;
        }

        /// <summary>
        /// Unloads the element and detaches definition change events.
        /// </summary>
        public virtual void Unload()
        {
            if (!IsLoaded)
                return;

            if (Definition != null)
            {
                // Unsubscribe from events to prevent memory leaks
                Definition.Changed -= Definition_Changed;
            }

            _isLoaded = false;
        }

        /// <summary>
        /// Handles a change from the definition.
        /// </summary>
        private void Definition_Changed(object? sender, DefinitionChangedEventArgs e)
        {
            if (e == null) return;

            // Apply only the changed property
            ApplyDefinitionChange(e.Key);
        }

        /// <summary>
        /// Applies the full definition to this element.
        /// Override in derived classes for custom behavior.
        /// </summary>
        protected virtual void ApplyDefinition()
        {
            Id = Definition.Id;
            Class = Definition.Class;
        }

        /// <summary>
        /// Applies a single changed property from the definition.
        /// Override in derived classes to handle incremental updates.
        /// </summary>
        /// <param name="key">The name of the property that changed.</param>
        protected virtual void ApplyDefinitionChange(string? key)
        {
            switch (key)
            {
                case nameof(IElementDefinition.Id):
                    Id = Definition.Id;
                    break;
                case nameof(IElementDefinition.Class):
                    Class = Definition.Class;
                    break;
            }
        }
    }
}