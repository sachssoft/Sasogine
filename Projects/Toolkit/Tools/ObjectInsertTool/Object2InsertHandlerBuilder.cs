using Sachssoft.Engine.Collections;
using System;

namespace Sachssoft.Engine.Components.Tools
{
    /// <summary>
    /// Provides a fluent builder for creating 2D object insert handlers.
    /// </summary>
    public sealed class Object2InsertHandlerBuilder
    {
        private readonly Action<IDefinition> _addDefinition;
        private readonly Func<IDefinition, bool> _removeDefinition;
        private readonly Func<IDefinition, IEngineObject?> _findObject;
        private readonly Func<Object2InsertContext, IDefinition> _create;
        private Object2InsertMode _mode = Object2InsertMode.Free;
        private Func<IDefinition, IEngineObject, Object2InsertMode>? _modeSelector;
        private Action<IDefinition, IEngineObject, Object2InsertContext>? _attached;
        private Action<IDefinition, Object2InsertContext>? _drag;
        private Action<IDefinition, Object2InsertContext>? _complete;
        private Action<IDefinition, Object2InsertContext>? _cancel;
        private Action<IDefinition, IEngineObject, Object2InsertContext>? _release;

        private Object2InsertHandlerBuilder(
            Action<IDefinition> addDefinition,
            Func<IDefinition, bool> removeDefinition,
            Func<IDefinition, IEngineObject?> findObject,
            Func<Object2InsertContext, IDefinition> create)
        {
            _addDefinition = addDefinition;
            _removeDefinition = removeDefinition;
            _findObject = findObject;
            _create = create;
        }

        /// <summary>
        /// Creates a new builder using definition and engine object binding collections.
        /// </summary>
        /// <typeparam name="TSourceDefinition">The definition type stored by the binding collection.</typeparam>
        /// <typeparam name="TObject">The engine object type exposed by the binding collection.</typeparam>
        /// <param name="definitionSource">The mutable source collection containing the definitions.</param>
        /// <param name="objectsSource">The read-only collection containing the bound engine objects.</param>
        /// <param name="callback">The callback used to create the definition when insertion begins.</param>
        /// <returns>A new builder instance.</returns>
        public static Object2InsertHandlerBuilder Create<TSourceDefinition, TObject>(
            IBindingCollection<TSourceDefinition> definitionSource,
            IReadOnlyBindingCollection<TObject> objectsSource,
            Func<Object2InsertContext, IDefinition> callback)
            where TSourceDefinition : class, IDefinition
            where TObject : class, IEngineObject
        {
            ArgumentNullException.ThrowIfNull(definitionSource);
            ArgumentNullException.ThrowIfNull(objectsSource);
            ArgumentNullException.ThrowIfNull(callback);

            return new Object2InsertHandlerBuilder(
                definition =>
                {
                    if (definition is not TSourceDefinition sourceDefinition)
                    {
                        throw new InvalidOperationException(
                            $"The definition must be assignable to '{typeof(TSourceDefinition).Name}'.");
                    }

                    definitionSource.Add(sourceDefinition);
                },
                definition => definition is TSourceDefinition sourceDefinition &&
                    definitionSource.Remove(sourceDefinition),
                definition =>
                {
                    for (int i = 0; i < objectsSource.Count; i++)
                    {
                        TObject obj = objectsSource[i];

                        if (ReferenceEquals(obj.Definition, definition))
                            return obj;
                    }

                    return null;
                },
                callback);
        }

        /// <summary>
        /// Sets the insertion mode.
        /// </summary>
        public Object2InsertHandlerBuilder WithMode(Object2InsertMode mode)
        {
            _mode = mode;
            _modeSelector = null;
            return this;
        }

        /// <summary>
        /// Sets a callback used to determine the insertion mode from the created definition
        /// and its bound engine object.
        /// </summary>
        public Object2InsertHandlerBuilder WithMode(
            Func<IDefinition, IEngineObject, Object2InsertMode> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _modeSelector = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback invoked after the bound engine object has been attached.
        /// </summary>
        public Object2InsertHandlerBuilder OnAttached(
            Action<IDefinition, IEngineObject, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _attached = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback invoked while the definition is being dragged.
        /// </summary>
        public Object2InsertHandlerBuilder OnDrag(Action<IDefinition, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _drag = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback invoked when insertion is completed.
        /// </summary>
        public Object2InsertHandlerBuilder OnComplete(Action<IDefinition, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _complete = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback invoked when insertion is canceled.
        /// </summary>
        public Object2InsertHandlerBuilder OnCancel(Action<IDefinition, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _cancel = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback invoked before the bound engine object is released.
        /// </summary>
        public Object2InsertHandlerBuilder OnRelease(
            Action<IDefinition, IEngineObject, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _release = callback;
            return this;
        }

        /// <summary>
        /// Builds the configured 2D object insert handler.
        /// </summary>
        public IObject2InsertHandler Build()
        {
            return new ObjectInsertHandler(
                _mode,
                _modeSelector,
                _addDefinition,
                _removeDefinition,
                _findObject,
                _create,
                _attached,
                _drag,
                _complete,
                _cancel,
                _release);
        }

        private sealed class ObjectInsertHandler : IObject2InsertHandler
        {
            private readonly Object2InsertMode _defaultMode;
            private readonly Func<IDefinition, IEngineObject, Object2InsertMode>? _modeSelector;
            private readonly Action<IDefinition> _addDefinition;
            private readonly Func<IDefinition, bool> _removeDefinition;
            private readonly Func<IDefinition, IEngineObject?> _findObject;
            private readonly Func<Object2InsertContext, IDefinition> _create;
            private readonly Action<IDefinition, IEngineObject, Object2InsertContext>? _attached;
            private readonly Action<IDefinition, Object2InsertContext>? _drag;
            private readonly Action<IDefinition, Object2InsertContext>? _complete;
            private readonly Action<IDefinition, Object2InsertContext>? _cancel;
            private readonly Action<IDefinition, IEngineObject, Object2InsertContext>? _release;
            private IDefinition? _activeDefinition;
            private IEngineObject? _activeObject;

            public ObjectInsertHandler(
                Object2InsertMode mode,
                Func<IDefinition, IEngineObject, Object2InsertMode>? modeSelector,
                Action<IDefinition> addDefinition,
                Func<IDefinition, bool> removeDefinition,
                Func<IDefinition, IEngineObject?> findObject,
                Func<Object2InsertContext, IDefinition> create,
                Action<IDefinition, IEngineObject, Object2InsertContext>? attached,
                Action<IDefinition, Object2InsertContext>? drag,
                Action<IDefinition, Object2InsertContext>? complete,
                Action<IDefinition, Object2InsertContext>? cancel,
                Action<IDefinition, IEngineObject, Object2InsertContext>? release)
            {
                _defaultMode = mode;
                _modeSelector = modeSelector;
                Mode = mode;
                _addDefinition = addDefinition;
                _removeDefinition = removeDefinition;
                _findObject = findObject;
                _create = create;
                _attached = attached;
                _drag = drag;
                _complete = complete;
                _cancel = cancel;
                _release = release;
            }

            public Object2InsertMode Mode { get; private set; }

            public IDefinition Create(Object2InsertContext context)
            {
                IDefinition definition = _create(context) ??
                    throw new InvalidOperationException("The create callback returned null.");

                _addDefinition(definition);

                IEngineObject obj = _findObject(definition) ??
                    throw new InvalidOperationException(
                        "The definition binding did not create an engine object for the inserted definition.");

                _activeDefinition = definition;
                _activeObject = obj;
                Mode = _modeSelector?.Invoke(definition, obj) ?? _defaultMode;
                _attached?.Invoke(definition, obj, context);

                return definition;
            }

            public void Drag(IDefinition definition, Object2InsertContext context)
            {
                _drag?.Invoke(definition, context);
            }

            public void Complete(IDefinition definition, Object2InsertContext context)
            {
                _complete?.Invoke(definition, context);
                ClearActive(definition);
            }

            public void Cancel(IDefinition definition, Object2InsertContext context)
            {
                IEngineObject? obj = ReferenceEquals(_activeDefinition, definition)
                    ? _activeObject
                    : _findObject(definition);

                _cancel?.Invoke(definition, context);

                if (obj is not null)
                    _release?.Invoke(definition, obj, context);

                _removeDefinition(definition);
                ClearActive(definition);
            }

            private void ClearActive(IDefinition definition)
            {
                if (!ReferenceEquals(_activeDefinition, definition))
                    return;

                _activeDefinition = null;
                _activeObject = null;
                Mode = _defaultMode;
            }
        }
    }
}
