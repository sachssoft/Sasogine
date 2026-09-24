using Sachssoft.Engine.Collections;
using System;

namespace Sachssoft.Engine.Components.Tools
{
    /// <summary>
    /// Provides a fluent builder for creating strongly typed 2D object insert handlers.
    /// </summary>
    /// <typeparam name="TDefinition">The type of definition handled by the insert handler.</typeparam>
    /// <typeparam name="TObject">The type of engine object bound to the definition.</typeparam>
    public sealed class Object2InsertHandlerBuilder<TDefinition, TObject>
        where TDefinition : class, IDefinition
        where TObject : class, IEngineObject
    {
        private readonly Action<TDefinition> _addDefinition;
        private readonly Func<TDefinition, bool> _removeDefinition;
        private readonly Func<TDefinition, TObject?> _findObject;
        private readonly Func<Object2InsertContext, TDefinition> _create;
        private Action<TDefinition, TObject, Object2InsertContext>? _attached;
        private Action<TDefinition, Object2InsertContext>? _drag;
        private Action<TDefinition, Object2InsertContext>? _complete;
        private Action<TDefinition, Object2InsertContext>? _cancel;
        private Action<TDefinition, TObject, Object2InsertContext>? _release;

        private Object2InsertHandlerBuilder(
            Action<TDefinition> addDefinition,
            Func<TDefinition, bool> removeDefinition,
            Func<TDefinition, TObject?> findObject,
            Func<Object2InsertContext, TDefinition> create)
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
        /// <param name="definitionSource">The mutable source collection containing the definitions.</param>
        /// <param name="objectsSource">The read-only collection containing the bound engine objects.</param>
        /// <param name="callback">The callback used to create the definition when insertion begins.</param>
        /// <returns>A new builder instance.</returns>
        public static Object2InsertHandlerBuilder<TDefinition, TObject> Create<TSourceDefinition>(
            IBindingCollection<TSourceDefinition> definitionSource,
            IReadOnlyBindingCollection<TObject> objectsSource,
            Func<Object2InsertContext, TDefinition> callback)
            where TSourceDefinition : class, IDefinition
        {
            ArgumentNullException.ThrowIfNull(definitionSource);
            ArgumentNullException.ThrowIfNull(objectsSource);
            ArgumentNullException.ThrowIfNull(callback);

            return new Object2InsertHandlerBuilder<TDefinition, TObject>(
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
        /// Sets the callback invoked after the bound engine object has been attached.
        /// </summary>
        public Object2InsertHandlerBuilder<TDefinition, TObject> OnAttached(
            Action<TDefinition, TObject, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _attached = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback invoked while the definition is being dragged.
        /// </summary>
        public Object2InsertHandlerBuilder<TDefinition, TObject> OnDrag(Action<TDefinition, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _drag = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback invoked when insertion is completed.
        /// </summary>
        public Object2InsertHandlerBuilder<TDefinition, TObject> OnComplete(Action<TDefinition, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _complete = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback invoked when insertion is canceled.
        /// </summary>
        public Object2InsertHandlerBuilder<TDefinition, TObject> OnCancel(Action<TDefinition, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _cancel = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback invoked before the bound engine object is released.
        /// </summary>
        public Object2InsertHandlerBuilder<TDefinition, TObject> OnRelease(
            Action<TDefinition, TObject, Object2InsertContext> callback)
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
            private readonly Action<TDefinition> _addDefinition;
            private readonly Func<TDefinition, bool> _removeDefinition;
            private readonly Func<TDefinition, TObject?> _findObject;
            private readonly Func<Object2InsertContext, TDefinition> _create;
            private readonly Action<TDefinition, TObject, Object2InsertContext>? _attached;
            private readonly Action<TDefinition, Object2InsertContext>? _drag;
            private readonly Action<TDefinition, Object2InsertContext>? _complete;
            private readonly Action<TDefinition, Object2InsertContext>? _cancel;
            private readonly Action<TDefinition, TObject, Object2InsertContext>? _release;
            private TDefinition? _activeDefinition;
            private TObject? _activeObject;

            public ObjectInsertHandler(
                Action<TDefinition> addDefinition,
                Func<TDefinition, bool> removeDefinition,
                Func<TDefinition, TObject?> findObject,
                Func<Object2InsertContext, TDefinition> create,
                Action<TDefinition, TObject, Object2InsertContext>? attached,
                Action<TDefinition, Object2InsertContext>? drag,
                Action<TDefinition, Object2InsertContext>? complete,
                Action<TDefinition, Object2InsertContext>? cancel,
                Action<TDefinition, TObject, Object2InsertContext>? release)
            {
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

            public IDefinition Create(Object2InsertContext context)
            {
                TDefinition definition = _create(context) ??
                    throw new InvalidOperationException("The create callback returned null.");

                _addDefinition(definition);

                TObject obj = _findObject(definition) ??
                    throw new InvalidOperationException(
                        "The definition binding did not create an engine object for the inserted definition.");

                _activeDefinition = definition;
                _activeObject = obj;
                _attached?.Invoke(definition, obj, context);

                return definition;
            }

            public void Drag(IDefinition definition, Object2InsertContext context)
            {
                _drag?.Invoke(GetDefinition(definition), context);
            }

            public void Complete(IDefinition definition, Object2InsertContext context)
            {
                TDefinition typedDefinition = GetDefinition(definition);
                _complete?.Invoke(typedDefinition, context);
                ClearActive(typedDefinition);
            }

            public void Cancel(IDefinition definition, Object2InsertContext context)
            {
                TDefinition typedDefinition = GetDefinition(definition);
                TObject? obj = ReferenceEquals(_activeDefinition, typedDefinition)
                    ? _activeObject
                    : _findObject(typedDefinition);

                _cancel?.Invoke(typedDefinition, context);

                if (obj is not null)
                    _release?.Invoke(typedDefinition, obj, context);

                _removeDefinition(typedDefinition);
                ClearActive(typedDefinition);
            }

            private void ClearActive(TDefinition definition)
            {
                if (!ReferenceEquals(_activeDefinition, definition))
                    return;

                _activeDefinition = null;
                _activeObject = null;
            }

            private static TDefinition GetDefinition(IDefinition definition)
            {
                return definition as TDefinition ??
                    throw new InvalidOperationException(
                        $"The definition must be of type '{typeof(TDefinition).Name}'.");
            }
        }
    }
}
