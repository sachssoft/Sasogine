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
    public abstract class EngineObject<TDefinition> : IEngineObject
        where TDefinition : class, IEngineObjectDefinition
    {
        private TDefinition? _definition;
        private bool _isLoaded;

        /// <summary>
        /// The definition instance that configures this element.
        /// Acts like the "Model" in a ViewModel-Model pattern: it is static configuration data,
        /// typically used for editor purposes or when loading from external sources.
        /// <para>
        /// Warning: During gameplay, avoid repeatedly accessing the Definition,
        /// as it may introduce performance overhead. It is recommended to copy or initialize
        /// all necessary fields from the definition at element creation/load time.
        /// </para>
        /// </summary>
        // Die Definition fungiert wie das "Model" im ViewModel-Model-Prinzip: 
        // Sie enthält statische Konfigurationsdaten und wird typischerweise für den Editor 
        // oder beim Laden aus externen Daten genutzt. 
        // Achtung: Während des Spiels sollte die Definition nicht dauerhaft abgefragt werden, 
        // da dies zu Performance-Einbußen führen kann. Alle notwendigen Felder sollten
        // beim Initialisieren oder Laden des Elements kopiert oder gesetzt werden.
        public TDefinition Definition
        {
            get
            {
                if (_definition == null)
                {
                    _definition = ResolveDefinition();

                    if (_definition == null)
                        throw new InvalidOperationException();
                }
                return _definition;
            }
        }

        IEngineObjectDefinition IEngineObject.Definition => Definition;

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

        //public void Initialize(TDefinition definition)
        //{
        //    if (Definition != null)
        //        throw new InvalidOperationException("Already initialized");

        //    Definition = definition;
        //}

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
                // ...
            }

            _isLoaded = false;
        }

        protected abstract TDefinition ResolveDefinition();

        /// <summary>
        /// Applies the full definition to this element.
        /// Override in derived classes for custom behavior.
        /// </summary>
        public virtual void ApplyDefinition()
        {
            Id = Definition.Id;
            Class = Definition.Class;
        }

        /// <summary>
        /// Applies a single changed property from the definition.
        /// Override in derived classes to handle incremental updates.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        public virtual void ApplyDefinitionChange(string? propertyName)
        {
            switch (propertyName)
            {
                case nameof(IEngineObjectDefinition.Id):
                    Id = Definition.Id;
                    break;
                case nameof(IEngineObjectDefinition.Class):
                    Class = Definition.Class;
                    break;
            }
        }
    }
}