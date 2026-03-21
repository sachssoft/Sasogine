using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Presentation
{
    /// <summary>
    /// Base class for all scenes in the presentation layer.
    /// Supports switching runtime at any time (auto unload/load).
    /// Runtime may be null.
    /// </summary>
    public abstract class SceneBase : IDisposeManagerProvider
    {
        private IPresentationHostElement? _container;
        private bool _isActive;
        private SceneManager? _scenes;
        private GameApplication? _application;
        private Type? _cachedType;
        private SceneSectionBase? _cachedSection;
        private readonly Dictionary<Type, SceneSectionBase> _sections = new();

        protected SceneBase()
        {
        }

        public GameApplication Application
        {
            get => _application ?? throw new InvalidOperationException("Scene is not attached to a GameApplication.");
        }

        public IPresentationHostElement? Container
        {
            get => _container;
            protected set => _container = value;
        }

        public DisposeManager DisposeManager { get; } = new DisposeManager();

        public SceneManager Scenes
        {
            get => _scenes ?? throw new InvalidOperationException("Scenes is not set.");
            internal set
            {
                _scenes = value;
                _application = _scenes.Application;
            }
        }

        public IPresentationHost? Host => Container?.Host;

        public bool IsActive
        {
            get => _isActive;
            internal set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    if (_isActive)
                        OnActivated();
                    else
                        OnDeactivated();
                }
            }
        }

        public bool IsLoaded { get; internal set; }

        protected void RegisterSection<TSection>(TSection section)
            where TSection : SceneSectionBase
        {
            var type = typeof(TSection);

            if (_sections.ContainsKey(type))
                throw new InvalidOperationException($"Scene section '{type.Name}' is already registered.");

            _sections[type] = section;
            DisposeManager.Register(section);
        }

        public TSection GetSection<TSection>() where TSection : SceneSectionBase
        {
            var type = typeof(TSection);

            // Hotpath: Cache-Treffer
            if (_cachedType == type)
            {
                if (_cachedSection is null)
                    throw new InvalidOperationException(
                        $"Scene section '{type.Name}' wurde gecached, ist aber null. " +
                        "Dies deutet auf einen Fehler in RegisterSection oder Cache-Invalidierung hin."
                    );

                // Safe Unsafe.Cast, da Typ durch _cachedType geprüft
                return Unsafe.As<SceneSectionBase, TSection>(ref _cachedSection);
            }

            // Coldpath: Dictionary-Lookup
            if (!_sections.TryGetValue(type, out var section))
                throw new InvalidOperationException($"Scene section '{type.Name}' ist nicht registriert.");

            if (section is null)
                throw new InvalidOperationException($"Scene section '{type.Name}' ist null. Dies ist ein Programmierfehler.");

            // Cache aktualisieren
            _cachedType = type;
            _cachedSection = section;

            // Hotpath-freundlich zurückgeben
            return Unsafe.As<SceneSectionBase, TSection>(ref _cachedSection);
        }

        protected void Leave()
        {
            if (!IsLoaded)
                return;

            IsActive = false;
            OnLeft();
        }

        /// <summary>
        /// Default implementation closes this scene via the SceneManager.
        /// </summary>
        protected virtual void OnLeft()
        {
            _scenes?.Close(this);
        }

        internal protected virtual void OnActivated() { }

        internal protected virtual void OnDeactivated() { }

        internal protected virtual void OnLoad()
        {
            IsLoaded = true;
        }

        internal protected virtual void OnUnload()
        {
            DisposeManager.Dispose();
            IsLoaded = false;
        }

        internal protected virtual void OnClientSizeChanged() { }

        internal virtual bool CanRender(PresentationContext context) => true;

        internal protected virtual void OnUpdate(PresentationContext context)
        {
        }

        internal protected virtual void OnDrawContent(PresentationContext context)
        {
        }

        internal protected virtual void OnDrawOverlay(PresentationContext context) { }

    }
}
