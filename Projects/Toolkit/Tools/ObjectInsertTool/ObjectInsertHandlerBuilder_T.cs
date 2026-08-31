using System;

namespace Sachssoft.Sasogine.Components.Tools
{

    /// <summary>
    /// Provides a fluent builder for creating strongly typed object insert handlers.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object handled during insertion.
    /// </typeparam>
    public sealed class ObjectInsertHandlerBuilder<T>
        where T : class
    {
        private Func<ObjectInsertContext, T>? _create;
        private Action<T, ObjectInsertContext>? _drag;
        private Action<T, ObjectInsertContext>? _complete;
        private Action<T, ObjectInsertContext>? _cancel;

        /// <summary>
        /// Sets the callback used to create an object when insertion begins.
        /// </summary>
        public ObjectInsertHandlerBuilder<T> Create(
            Func<ObjectInsertContext, T> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            _create = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback used while the object is being dragged.
        /// </summary>
        public ObjectInsertHandlerBuilder<T> Drag(
            Action<T, ObjectInsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            _drag = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback used when insertion is completed.
        /// </summary>
        public ObjectInsertHandlerBuilder<T> Complete(
            Action<T, ObjectInsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            _complete = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback used when insertion is canceled.
        /// </summary>
        public ObjectInsertHandlerBuilder<T> Cancel(
            Action<T, ObjectInsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            _cancel = callback;
            return this;
        }

        /// <summary>
        /// Builds the object insert handler.
        /// </summary>
        public IObjectInsertHandler Build()
        {
            if (_create == null)
            {
                throw new InvalidOperationException(
                    "A create callback must be specified.");
            }

            return new ObjectInsertHandler(
                _create,
                _drag,
                _complete,
                _cancel);
        }

        private sealed class ObjectInsertHandler : IObjectInsertHandler
        {
            private readonly Func<ObjectInsertContext, T> _create;
            private readonly Action<T, ObjectInsertContext>? _drag;
            private readonly Action<T, ObjectInsertContext>? _complete;
            private readonly Action<T, ObjectInsertContext>? _cancel;

            public ObjectInsertHandler(
                Func<ObjectInsertContext, T> create,
                Action<T, ObjectInsertContext>? drag,
                Action<T, ObjectInsertContext>? complete,
                Action<T, ObjectInsertContext>? cancel)
            {
                _create = create;
                _drag = drag;
                _complete = complete;
                _cancel = cancel;
            }

            public object Create(ObjectInsertContext context)
            {
                return _create(context) ??
                    throw new InvalidOperationException(
                        "The create callback returned null.");
            }

            public void Drag(
                object value,
                ObjectInsertContext context)
            {
                _drag?.Invoke(
                    GetValue(value),
                    context);
            }

            public void Complete(
                object value,
                ObjectInsertContext context)
            {
                _complete?.Invoke(
                    GetValue(value),
                    context);
            }

            public void Cancel(
                object value,
                ObjectInsertContext context)
            {
                _cancel?.Invoke(
                    GetValue(value),
                    context);
            }

            private static T GetValue(object value)
            {
                if (value is not T typedValue)
                {
                    throw new InvalidOperationException(
                        $"The inserted object must be of type '{typeof(T).FullName}'.");
                }

                return typedValue;
            }
        }
    }
}