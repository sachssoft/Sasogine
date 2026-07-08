using Sachssoft.Sasogine.Components.Models;
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
    public abstract class EngineObject<TDefinition> : EngineObjectBase, IEngineObject
        where TDefinition : class, IDefinition
    {
        private TDefinition? _definition;

        public EngineObject()
        {
            _definition = null;
        }

        public EngineObject(TDefinition definition)
        {
            _definition = definition;
        }

        public event EventHandler<EngineObjectChangedEventArgs>? IdChanged;
        public event EventHandler<EngineObjectChangedEventArgs>? ClassChanged;

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
                EnsureDefinition();
                return _definition;
            }
        }

        IDefinition? IEngineObject.Definition => Definition;

        public override void Load()
        {
            if (IsLoaded)
                return;

            if (Definition != null)
            {
                // Apply all properties from the definition
                ConfigureFromDefinitionInternal();
                ConfigureFromDefinition();
            }

            IsLoaded = true;
        }

        public override async Task LoadAsync()
        {
            if (IsLoaded)
            {
                await Task.CompletedTask;
                return;
            }

            if (Definition != null)
            {
                // Apply all properties from the definition
                ConfigureFromDefinitionInternal();
                ConfigureFromDefinition();
            }

            IsLoaded = true;

            await Task.CompletedTask;
            return;
        }

        /// <summary>
        /// Unloads the element and detaches definition change events.
        /// </summary>
        public override void Unload()
        {
            if (!IsLoaded)
                return;

            IsLoaded = false;
        }

        [MemberNotNull(nameof(_definition))]
        public void EnsureDefinition()
        {
            if (_definition != null)
                return;

            _definition = ResolveDefinition()
                ?? throw new InvalidOperationException(
                    $"ResolveDefinition returned null for {GetType().Name}");
        }

        protected virtual TDefinition ResolveDefinition()
        {
            return _definition!;
        }

        /// <summary>
        /// Applies the full definition to this element.
        /// Override in derived classes for custom behavior.
        /// </summary>
        protected virtual void ConfigureFromDefinition()
        {
        }

        private void ConfigureFromDefinitionInternal()
        {
            if (Definition is IEngineObjectDefinition eod)
            {
                if (!ReferenceEquals(Id, eod.Id))
                {
                    var oldId = Id;
                    Id = eod.Id;
                    if (IdChanged != null)
                        IdChanged(this, new EngineObjectChangedEventArgs(oldId, Id, Class, Class));
                }

                if (!ReferenceEquals(Class, eod.Class))
                {
                    var oldClass = Class;
                    Class = eod.Class;
                    if (ClassChanged != null)
                        ClassChanged(this, new EngineObjectChangedEventArgs(Id, Id, oldClass, Class));
                }
            }
        }
    }
}
