using System;

namespace Sachssoft.Sasogine.Components.Tools
{

    /// <summary>
    /// Provides a fluent builder for creating object insert handlers.
    /// </summary>
    public sealed class ObjectInsertHandlerBuilder
    {
        private Func<ObjectInsertContext, object>? _create;
        private Action<object, ObjectInsertContext>? _drag;
        private Action<object, ObjectInsertContext>? _complete;
        private Action<object, ObjectInsertContext>? _cancel;

        /// <summary>
        /// Sets the callback used to create an object when insertion begins.
        /// </summary>
        public ObjectInsertHandlerBuilder Create(
            Func<ObjectInsertContext, object> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _create = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback used while the object is being dragged.
        /// </summary>
        public ObjectInsertHandlerBuilder Drag(
            Action<object, ObjectInsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _drag = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback used when insertion is completed.
        /// </summary>
        public ObjectInsertHandlerBuilder Complete(
            Action<object, ObjectInsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            _complete = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback used when insertion is canceled.
        /// </summary>
        public ObjectInsertHandlerBuilder Cancel(
            Action<object, ObjectInsertContext> callback)
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
            private readonly Func<ObjectInsertContext, object> _create;
            private readonly Action<object, ObjectInsertContext>? _drag;
            private readonly Action<object, ObjectInsertContext>? _complete;
            private readonly Action<object, ObjectInsertContext>? _cancel;

            public ObjectInsertHandler(
                Func<ObjectInsertContext, object> create,
                Action<object, ObjectInsertContext>? drag,
                Action<object, ObjectInsertContext>? complete,
                Action<object, ObjectInsertContext>? cancel)
            {
                _create = create;
                _drag = drag;
                _complete = complete;
                _cancel = cancel;
            }

            public object Create(ObjectInsertContext context)
            {
                return _create(context);
            }

            public void Drag(
                object value,
                ObjectInsertContext context)
            {
                _drag?.Invoke(
                    value,
                    context);
            }

            public void Complete(
                object value,
                ObjectInsertContext context)
            {
                _complete?.Invoke(
                    value,
                    context);
            }

            public void Cancel(
                object value,
                ObjectInsertContext context)
            {
                _cancel?.Invoke(
                    value,
                    context);
            }
        }
    }
}