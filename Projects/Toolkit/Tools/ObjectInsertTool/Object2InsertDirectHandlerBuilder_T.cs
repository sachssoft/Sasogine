using System;

namespace Sachssoft.Engine.Components.Tools
{
    /// <summary>
    /// Provides a fluent builder for creating strongly typed 2D object insert handlers
    /// that directly create and manage engine objects during insertion.
    /// </summary>
    /// <typeparam name="TDefinition">The type of definition handled by the insert handler.</typeparam>
    /// <typeparam name="TObject">The type of engine object created for the definition.</typeparam>
    public sealed class Object2InsertDirectHandlerBuilder<TDefinition, TObject>
        where TDefinition : class, IDefinition
        where TObject : class, IEngineObject
    {
        private readonly Func<TDefinition, TObject> _createObject;
        private readonly Func<Object2InsertContext, TDefinition> _createDefinition;
        private Object2InsertMode _mode = Object2InsertMode.Free;
        private Func<TDefinition, TObject, Object2InsertMode>? _modeSelector;
        private Action<TDefinition, TObject, Object2InsertContext>? _attached;
        private Action<TDefinition, Object2InsertContext>? _drag;
        private Action<TDefinition, Object2InsertContext>? _complete;
        private Action<TDefinition, Object2InsertContext>? _cancel;
        private Action<TDefinition, TObject, Object2InsertContext>? _release;

        private Object2InsertDirectHandlerBuilder(Func<TDefinition, TObject> createObject,
            Func<Object2InsertContext, TDefinition> createDefinition)
        {
            _createObject = createObject;
            _createDefinition = createDefinition;
        }

        /// <summary>
        /// Creates a new builder.
        /// </summary>
        /// <param name="createObject">The callback used to create an engine object from a definition.</param>
        /// <param name="createDefinition">The callback used to create the definition when insertion begins.</param>
        /// <returns>A new builder instance.</returns>
        public static Object2InsertDirectHandlerBuilder<TDefinition, TObject> Create(
            Func<TDefinition, TObject> createObject, Func<Object2InsertContext, TDefinition> createDefinition)
        {
            ArgumentNullException.ThrowIfNull(createObject);
            ArgumentNullException.ThrowIfNull(createDefinition);
            return new Object2InsertDirectHandlerBuilder<TDefinition, TObject>(createObject, createDefinition);
        }

        /// <summary>Sets the insertion mode.</summary>
        public Object2InsertDirectHandlerBuilder<TDefinition, TObject> WithMode(Object2InsertMode mode)
        {
            _mode = mode;
            _modeSelector = null;
            return this;
        }

        /// <summary>Sets a callback used to determine the insertion mode from the created definition and engine object.</summary>
        public Object2InsertDirectHandlerBuilder<TDefinition, TObject> WithMode(
            Func<TDefinition, TObject, Object2InsertMode> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _modeSelector = callback;
            return this;
        }

        /// <summary>Sets the callback invoked after the engine object has been created.</summary>
        public Object2InsertDirectHandlerBuilder<TDefinition, TObject> OnAttached(
            Action<TDefinition, TObject, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _attached = callback;
            return this;
        }

        /// <summary>Sets the callback invoked while the definition is being dragged.</summary>
        public Object2InsertDirectHandlerBuilder<TDefinition, TObject> OnDrag(Action<TDefinition, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _drag = callback;
            return this;
        }

        /// <summary>Sets the callback invoked when insertion is completed.</summary>
        public Object2InsertDirectHandlerBuilder<TDefinition, TObject> OnComplete(Action<TDefinition, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _complete = callback;
            return this;
        }

        /// <summary>Sets the callback invoked when insertion is canceled.</summary>
        public Object2InsertDirectHandlerBuilder<TDefinition, TObject> OnCancel(Action<TDefinition, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _cancel = callback;
            return this;
        }

        /// <summary>Sets the callback invoked when the engine object is released.</summary>
        public Object2InsertDirectHandlerBuilder<TDefinition, TObject> OnRelease(
            Action<TDefinition, TObject, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _release = callback;
            return this;
        }

        /// <summary>Builds the configured 2D object insert handler.</summary>
        public IObject2InsertHandler Build() => new ObjectInsertHandler(_createObject, _createDefinition,
            _mode, _modeSelector, _attached, _drag, _complete, _cancel, _release);

        private sealed class ObjectInsertHandler : IObject2InsertHandler
        {
                private readonly Func<TDefinition, TObject> _createObject;
            private readonly Func<Object2InsertContext, TDefinition> _createDefinition;
            private readonly Object2InsertMode _defaultMode;
            private readonly Func<TDefinition, TObject, Object2InsertMode>? _modeSelector;
            private readonly Action<TDefinition, TObject, Object2InsertContext>? _attached;
            private readonly Action<TDefinition, Object2InsertContext>? _drag;
            private readonly Action<TDefinition, Object2InsertContext>? _complete;
            private readonly Action<TDefinition, Object2InsertContext>? _cancel;
            private readonly Action<TDefinition, TObject, Object2InsertContext>? _release;
            private TDefinition? _activeDefinition;
            private TObject? _activeObject;

            public ObjectInsertHandler(Func<TDefinition, TObject> createObject,
                Func<Object2InsertContext, TDefinition> createDefinition, Object2InsertMode mode,
                Func<TDefinition, TObject, Object2InsertMode>? modeSelector,
                Action<TDefinition, TObject, Object2InsertContext>? attached,
                Action<TDefinition, Object2InsertContext>? drag, Action<TDefinition, Object2InsertContext>? complete,
                Action<TDefinition, Object2InsertContext>? cancel,
                Action<TDefinition, TObject, Object2InsertContext>? release)
            {
                _createObject = createObject;
                _createDefinition = createDefinition;
                _defaultMode = mode;
                _modeSelector = modeSelector;
                _attached = attached;
                _drag = drag;
                _complete = complete;
                _cancel = cancel;
                _release = release;
                Mode = mode;
            }

            public Object2InsertMode Mode { get; private set; }

            public IDefinition Create(Object2InsertContext context)
            {
                TDefinition definition = _createDefinition(context) ??
                    throw new InvalidOperationException("The create definition callback returned null.");
                TObject obj = _createObject(definition) ??
                    throw new InvalidOperationException("The create object callback returned null.");

                _activeDefinition = definition;
                _activeObject = obj;
                Mode = _modeSelector?.Invoke(definition, obj) ?? _defaultMode;
                _attached?.Invoke(definition, obj, context);
                return definition;
            }

            public void Drag(IDefinition definition, Object2InsertContext context) =>
                _drag?.Invoke(GetDefinition(definition), context);

            public void Complete(IDefinition definition, Object2InsertContext context)
            {
                TDefinition typedDefinition = GetDefinition(definition);
                _complete?.Invoke(typedDefinition, context);
                ClearActive(typedDefinition);
            }

            public void Cancel(IDefinition definition, Object2InsertContext context)
            {
                TDefinition typedDefinition = GetDefinition(definition);
                if (!ReferenceEquals(_activeDefinition, typedDefinition) || _activeObject is null)
                    throw new InvalidOperationException("The definition is not the active insertion definition.");

                TObject obj = _activeObject;
                _cancel?.Invoke(typedDefinition, context);
                _release?.Invoke(typedDefinition, obj, context);
                ClearActive(typedDefinition);
            }

            private void ClearActive(TDefinition definition)
            {
                if (!ReferenceEquals(_activeDefinition, definition))
                    return;
                _activeDefinition = null;
                _activeObject = null;
                Mode = _defaultMode;
            }

            private static TDefinition GetDefinition(IDefinition definition) => definition as TDefinition ??
                throw new InvalidOperationException($"The definition must be of type '{typeof(TDefinition).Name}'.");
        }
    }
}
